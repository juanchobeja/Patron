using Application.Abstractions.Messaging;
using Application.Events;
using Core.Abstractions;
using Core.Common;
using Core.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands
{
    public class ProcessPdfCommandHandler : ICommandHandler<ProcessPdfCommand>
    {
        private readonly IFileTrazabilidadRepository _trazaRepo;
        private readonly IMediator _mediator;   // inyectamos IMediator
        private readonly ILogger<ProcessPdfCommandHandler> _logger;

        public ProcessPdfCommandHandler(
            IFileTrazabilidadRepository trazaRepo,
            IMediator mediator,
            ILogger<ProcessPdfCommandHandler> logger)
        {
            _trazaRepo = trazaRepo;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result> Handle(ProcessPdfCommand request, CancellationToken ct)
        {
            var fileInfo = new FileInfo(request.Archivo);
            var nombreOriginal = fileInfo.Name;
            var correlationId = Guid.NewGuid();
            var guid = correlationId;
            var newName = $"{guid}.pdf";

            // 1) Obtenemos la carpeta destino usando tu método de dominio

            try
            {
                string destino = request.Cliente.ObtenerRutaDestino((TipoOrigen)request.Origen);
                if (string.IsNullOrWhiteSpace(destino))
                    throw new InvalidOperationException(
                        $"No configurada RutaDestino para origen {request.Origen}");

                Directory.CreateDirectory(destino);
                var destPath = Path.Combine(destino, newName);
                fileInfo.MoveTo(destPath);

                // Guarda trazabilidad
                var traza = new FileTrazabilidad(
                    0,
                    nombreOriginal,
                    guid,
                    DateTime.Now,
                    null,
                    request.Cliente.Nombre,
                    request.Origen
                );
                await _trazaRepo.AddAsync(traza, ct);

                // Publica evento éxito
                await _mediator.Publish(
                    new FileProcessedEvent(traza), ct);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moviendo {Archivo}", request.Archivo);
                //await MoverAError(request, fileInfo, correlationId, ct, ex);
                var errorDir = request.Cliente.RutaError;
                if (!string.IsNullOrWhiteSpace(errorDir))
                {
                    try
                    {
                        Directory.CreateDirectory(errorDir);
                        fileInfo.MoveTo(Path.Combine(errorDir, nombreOriginal), overwrite: true);
                    }
                    catch (Exception mvEx)
                    {
                        _logger.LogError(mvEx, "Falló mover a error");
                    }
                }

                // Evento fallo
                await _mediator.Publish(
                    new FileProcessingFailedEvent(
                        request.Archivo,
                        ex.Message,
                        request.Cliente.Nombre,
                        request.Origen
                    ), ct);

                // Log BD
                var logCmd = new CreateLogCommand(
                    nombreOriginal,
                    ex.Message,
                    "WorkerService",
                    "ERROR",
                    ex.ToString(),
                    correlationId
                );
                await _mediator.Send(logCmd, ct);

                return Result.Failure(new Error(
                    "ProcessPdfError",
                    ex.Message));
            }
        }

    }
}
