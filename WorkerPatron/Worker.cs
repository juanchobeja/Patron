using Application.Commands;
using Application.Models;
using Core.Abstractions;
using Core.Entities;
using FluentValidation;
using MediatR;
using System.Threading.Channels;

namespace WorkerPatron
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Channel<PdfProcessItem> _channel;
        private const int MaxDegreeOfParallelism = 4;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _channel = Channel.CreateUnbounded<PdfProcessItem>(new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = true
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Arrancamos los consumidores
            var consumers = Enumerable
                .Range(0, MaxDegreeOfParallelism)
                .Select(_ => Task.Run(() => ConsumeChannelAsync(stoppingToken), stoppingToken))
                .ToArray();

            // Productor: escanea rutas cada 10s
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var clientesRepo = scope.ServiceProvider.GetRequiredService<IClientesRepository>();
                var mediatorForLogging = scope.ServiceProvider.GetRequiredService<IMediator>();

                var clientes = await clientesRepo.GetClientesActivosAsync(stoppingToken);
                foreach (var cliente in clientes)
                {
                    // Recorremos cada origen definido en tu enum
                    foreach (TipoOrigen origen in Enum.GetValues<TipoOrigen>())
                    {
                        var ruta = cliente.ObtenerRutaOrigen(origen);
                        if (string.IsNullOrWhiteSpace(ruta))
                            continue;

                        if (string.IsNullOrWhiteSpace(ruta) || !Directory.Exists(ruta))
                        {
                            // Registra en BD vía CreateLogCommand
                            var logCmd = new CreateLogCommand(
                                Archivo: nameof(Worker),
                                Mensaje: $"Ruta no encontrada: {ruta}",
                                Servicio: "WorkerService",
                                Level: "ERROR",
                                Exception: null,
                                CorrelationId: Guid.NewGuid()
                            );
                            await mediatorForLogging.Send(logCmd, stoppingToken);
                            continue;
                        }

                        // Encolamos cada PDF encontrado
                        foreach (var archivo in Directory.GetFiles(ruta, "*.pdf"))
                        {
                            //var item = new PdfProcessItem(
                            //    Cliente: cliente,
                            //    Origen: (int)origen,
                            //    Archivo: archivo
                            //);
                            //await _channel.Writer.WriteAsync(item, stoppingToken);
                            await _channel.Writer.WriteAsync(
                                new PdfProcessItem(cliente, (int)origen, archivo),
                                stoppingToken
                            );
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }

            // Cerramos canal y esperamos consumidores
            _channel.Writer.Complete();
            await Task.WhenAll(consumers);
        }

        
        private async Task ConsumeChannelAsync(CancellationToken ct)
        {
            await foreach (var item in _channel.Reader.ReadAllAsync(ct))
            {
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                try
                {
                    var command = new ProcessPdfCommand(
                        Cliente: item.Cliente,
                        Origen: item.Origen,
                        Archivo: item.Archivo
                    );
                    await mediator.Send(command, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Worker fallo procesando {Archivo}", item.Archivo);
                }
            }
        }
    }
}