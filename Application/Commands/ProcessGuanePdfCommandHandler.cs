using Application.Abstractions.Messaging;
using Application.Events;
using Core.Abstractions;
using Core.Common;
using Core.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using WebSupergoo.ABCpdf11;

namespace Application.Commands
{
    public class ProcessGuanePdfCommandHandler : ICommandHandler<ProcessGuanePdfCommand>
    {
        private readonly IUnitOfWork _uow;
        private readonly IPublisher _publisher;
        private readonly IMediator _mediator;
        private readonly ILogger<ProcessGuanePdfCommandHandler> _logger;

        public ProcessGuanePdfCommandHandler(
            IUnitOfWork uow,
            IPublisher publisher,
            ILogger<ProcessGuanePdfCommandHandler> logger,
            IMediator mediator)
        {
            _uow = uow;
            _publisher = publisher;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<Result> Handle(ProcessGuanePdfCommand req, CancellationToken ct)
        {            
            try
            {
                // 1) Cruce Format_File ↔ Trazabilidad                
                var genId = Guid.Parse(Path.GetFileNameWithoutExtension(req.ArchivoPath));

                var fmt = await _uow.FormatFileRepository.GetByNombreOriginalAsync(genId, ct);
                if (fmt is  null)
                {
                    if (fmt is null)
                        return await HandleIntegrationErrorAsync(
                            mensaje: "No se encontró registro en FormatFile para este archivo",
                            archivoPath: req.ArchivoPath,
                            rutaError: req.Cliente.RutaError,
                            ct);                   
                }
                await _uow.TrazabilidadRepository.UpdateNombreGuaneAsync(
                        fmt.NombreOriginal, fmt.GuaneId, ct);

                var traz = await _uow.TrazabilidadRepository
                              .GetByNombreArchivoGeneradoAsync(genId, ct);
                if (traz is null || traz.Cliente != req.Cliente.Nombre)
                {
                    var motivo = traz is null
                        ? "No existe registro en trazabilidad tras UpdateNombreGuane"
                        : $"Cliente en trazabilidad ('{traz.Cliente}') no coincide con '{req.Cliente.Nombre}'";

                    return await HandleIntegrationErrorAsync(
                        mensaje: motivo,
                        archivoPath: req.ArchivoPath,
                        rutaError: req.Cliente.RutaError,
                        ct);
                }


                // 2) Obtener batch consecutivo
                var batchConsec = await _uow.ConfiguradorIntegradorRepository.GetAndIncrementBatchConsecutiveAsync(ct);
                //var batch = new Batch(Guid.NewGuid(), batchConsec, (int)req.Origen, DateTime.Now);
                var horaActual = DateTime.Now.Hour;

                var horaInicio = await _uow.ConfiguradorIntegradorRepository.GetHoraConfiguradaAsync("FechaInicioProceso", ct);
                var horaFin = await _uow.ConfiguradorIntegradorRepository.GetHoraConfiguradaAsync("FechaFinProceso", ct);

                DateTime fechaProceso;

                if (horaActual < horaInicio || horaActual >= horaFin)
                {
                    // Si está fuera del rango permitido, buscar siguiente día hábil
                    var siguienteDia = await _uow.FestivosRepository.ObtenerSiguienteDiaHabilAsync(DateTime.Today, ct);
                    fechaProceso = new DateTime(siguienteDia.Year, siguienteDia.Month, siguienteDia.Day, horaInicio, 0, 0);
                }
                else
                {
                    fechaProceso = DateTime.Now;
                }

                var batch = new Batch(Guid.NewGuid(), batchConsec, (int)req.Origen, fechaProceso);

                await _uow.BatchRepository.AddAsync(batch, ct);

                // 3) Obtener y reservar etiqueta
                var ce = await _uow.ConsecutivoEtiquetaRepository.GetAndIncrementAsync(req.Cliente.Nombre, ct);
                string etiqueta = ce.ConsecutivoEtiqueta ?? throw new InvalidOperationException("No se generó la etiqueta correctamente.");

                
                var destDir = Path.Combine(req.Cliente.RutaAlmacenada, DateTime.Now.ToString("yyyyMMdd"));


                // 4) Registrar Solicitud
                var sol = new Solicitud(
                    batch.BatchId,
                    fmt?.GuaneId ?? genId,
                    etiqueta,
                    req.Origen == TipoOrigen.Masivos,
                    Path.GetFileName(req.ArchivoPath)
                );
                await _uow.SolicitudRepository.AddAsync(sol, ct);

                // 5) Registrar Documento
                var doc = new Documento(
                    Guid.NewGuid(),
                    batch.BatchId,
                    sol.SolicitudId,
                    Path.Combine(destDir, Path.GetFileName(req.ArchivoPath)),
                    Path.Combine(destDir, Path.GetFileName(req.ArchivoPath))
                );
                await _uow.DocumentoRepository.AddAsync(doc, ct);

                // 6) Confirmar todo en BD
                await _uow.SaveChangesAsync(ct);

                // 7) Aplicar etiqueta en el PDF
                ApplyLabel(req.ArchivoPath, etiqueta);

                // 8) Mover a RutaAlmacenada/yyyyMMdd

                Directory.CreateDirectory(destDir);
                
                File.Move(req.ArchivoPath, Path.Combine(destDir, Path.GetFileName(req.ArchivoPath)));

                // 9) Publicar éxito
                await _publisher.Publish(
                        new FileProcessedEvent(
                            new FileTrazabilidad(
                                Id: 0,
                                Path.GetFileName(req.ArchivoPath),
                                genId,
                                DateTime.Now,
                                fmt?.GuaneId ?? Guid.Empty,
                                req.Cliente.Nombre,
                                (int)req.Origen
                            )
                   ), ct);

                return Result.Success();
            }
            catch (Exception ex)
            {
                // 10) Rollback automático y log de error
                _logger.LogError(ex, "Error completo en ProcessGuanePdfCommandHandler");
                await _publisher.Publish(
                    new FileProcessingFailedEvent(
                        req.ArchivoPath,
                        ex.Message,
                        req.Cliente.Nombre,
                        (int)req.Origen
                    ), ct);

                // Adicional: guardar en tabla de logs
                var logCmd = new CreateLogCommand(
                                    Path.GetFileName(req.ArchivoPath),
                                    ex.Message,
                                    "GuaneWorker",
                                    "ERROR",
                                    ex.ToString(),
                                    Guid.NewGuid()
                                );
                await _mediator.Send(logCmd, ct);

                //throw; // para que Host note el fallo
                return Result.Failure(new Error("ProcessGuaneError", ex.Message));
            }
        }

        /// <summary>
        /// Centraliza el log y el movimiento a carpeta de error.
        /// </summary>
        private async Task<Result> HandleIntegrationErrorAsync(
            string mensaje,
            string archivoPath,
            string rutaError,
            CancellationToken ct)
        {
            // 1) Log en BD
            await _mediator.Send(new CreateLogCommand(
                Archivo: Path.GetFileName(archivoPath),
                Mensaje: mensaje,
                Servicio: "GuaneWorker",
                Level: "ERROR",
                Exception: null,
                CorrelationId: Guid.NewGuid()
            ), ct);

            // 2) Mover a carpeta de error
            Directory.CreateDirectory(rutaError);
            var destinoError = Path.Combine(rutaError, Path.GetFileName(archivoPath));
            if (File.Exists(destinoError)) File.Delete(destinoError);
            File.Move(archivoPath, destinoError,true);

            return Result.Failure(new Error("ProcessGuaneError", mensaje));
        }


        private void ApplyLabel(string path, string text)
        {
            var doc = new Doc();
            string tempPath = null;

            try
            {
                // Cargar el PDF
                doc.Read(path);

                // Top-left
                doc.Rect.Inset(10, 10);
                doc.AddText(text);

                // Bottom-right
                var w = doc.MediaBox.Width;
                var h = doc.MediaBox.Height;
                doc.Rect.String = $"{w - 200} {h - 50} {w - 10} {h - 10}";
                doc.AddText(text);

                // Guardar en temp
                var dir = Path.GetDirectoryName(path)!;
                tempPath = Path.Combine(dir, $"{Guid.NewGuid()}.pdf");
                doc.Save(tempPath);
            }
            finally
            {
                // Siempre libera recursos de ABCpdf
                try { doc.Clear(); }
                catch { /* swallow */ }
            }

            // Reemplazar archivo original
            if (tempPath != null)
            {
                // Borrar el original (libera manejadores)
                File.Delete(path);
                // Mover el temp a la ruta original
                File.Move(tempPath, path);
            }
        }
    }

}
