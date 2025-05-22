using Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Observers
{
    public class FileProcessedEventHandler : INotificationHandler<FileProcessedEvent>
    {
        private readonly ILogger<FileProcessedEventHandler> _logger;

        public FileProcessedEventHandler(ILogger<FileProcessedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(FileProcessedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Archivo procesado correctamente: {Archivo} para Cliente {Cliente}",
                notification.Trazabilidad.NombreArchivoGenerado, notification.Trazabilidad.Cliente);
            return Task.CompletedTask;
        }
    }
}
