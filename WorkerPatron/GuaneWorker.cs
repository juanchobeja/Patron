using Application.Commands;
using Core.Abstractions;
using Core.Entities;
using MediatR;
using System.Threading.Channels;

namespace WorkerPatron
{
    public class GuaneWorker : BackgroundService
    {
        private readonly ILogger<GuaneWorker> _logger;
        private readonly IServiceScopeFactory _scopes;
        private readonly Channel<ProcessGuanePdfCommand> _channel;
        private const int MaxDegreeOfParallelism = 4;

        public GuaneWorker(ILogger<GuaneWorker> logger, IServiceScopeFactory scopes)
        {
            _logger = logger;
            _scopes = scopes;
            _channel = Channel.CreateUnbounded<ProcessGuanePdfCommand>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumers = Enumerable.Range(0, MaxDegreeOfParallelism)
                .Select(_ => Task.Run(() => Consume(stoppingToken), stoppingToken))
                .ToArray();

            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopes.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IClientesRepository>();
                //var mediatr = scope.ServiceProvider.GetRequiredService<IMediator>();
                var clientes = await repo.GetClientesActivosAsync(stoppingToken);

                foreach (var c in clientes)
                {
                    foreach (TipoOrigen origen in new[] { TipoOrigen.Sftp, TipoOrigen.Email })
                    {
                        var carpeta = c.ObtenerRutaDestinoGuane(origen);
                        if (!Directory.Exists(carpeta)) continue;

                        foreach (var file in Directory.GetFiles(carpeta, "*.pdf"))
                        {
                            if (IsFileLocked(file))
                                continue;   // Skip locked files this cycle
                                            //var cmd = new ProcessGuanePdfCommand(c, origen, file);
                                            //await _channel.Writer.WriteAsync(cmd, stoppingToken);
                            await _channel.Writer.WriteAsync(
                              new ProcessGuanePdfCommand(c, origen, file),
                              stoppingToken
                          );
                        }
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
            _channel.Writer.Complete();
            await Task.WhenAll(consumers);
        }

        private async Task Consume(CancellationToken ct)
        {
            //using var scope = _scopes.CreateScope();
            //var mediatr = scope.ServiceProvider.GetRequiredService<IMediator>();

            await foreach (var cmd in _channel.Reader.ReadAllAsync(ct))
            {
                using var scope = _scopes.CreateScope();
                var mediatr = scope.ServiceProvider.GetRequiredService<IMediator>();                
                try
                {
                    await mediatr.Send(cmd, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "GuaneWorker fallo en cmd");
                }
            }
        }

        private bool IsFileLocked(string path)
        {
            try
            {
                using var stream = new FileStream(path,
                    FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return false;
            }
            catch (IOException)
            {
                // Está siendo usado por otro proceso/hilo
                return true;
            }
        }

    }
}
