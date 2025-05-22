using Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Observers
{
    public class FileProcessingFailedEventHandler : INotificationHandler<FileProcessingFailedEvent>
    {
        private readonly ILogger<FileProcessingFailedEventHandler> _logger;

        public FileProcessingFailedEventHandler(ILogger<FileProcessingFailedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(FileProcessingFailedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogError("Error procesando archivo: {Archivo} - {Error} - Cliente: {Cliente} Origen: {Origen}",
                notification.Archivo, notification.Error, notification.Cliente, notification.Origen);
            return Task.CompletedTask;
        }
    }
}
