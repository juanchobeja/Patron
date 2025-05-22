using Core.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers
{
    public class LogCreatedEventHandler : INotificationHandler<LogCreatedEvent>
    {
        private readonly ILogger<LogCreatedEventHandler> _logger;

        public LogCreatedEventHandler(ILogger<LogCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(LogCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Log creado: Id={Id}, Servicio={Servicio}, Level={Level}, Mensaje={Mensaje}",
                notification.Log.Id,
                notification.Log.Servicio,
                notification.Log.Level,
                notification.Log.Mensaje
            );
            
            return Task.CompletedTask;
        }
    }
}