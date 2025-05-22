using Core.Entities;
using MediatR;

namespace Application.Events
{
    public record FileProcessedEvent(FileTrazabilidad Trazabilidad) : INotification;
}
