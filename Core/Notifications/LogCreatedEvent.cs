using Core.Entities;
using MediatR;

namespace Core.Notifications
{
    public record LogCreatedEvent(LogRecord Log) : INotification;
}