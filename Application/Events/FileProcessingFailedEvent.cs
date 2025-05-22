using MediatR;

namespace Application.Events
{
    public record FileProcessingFailedEvent(
        string Archivo,
        string Error,
        string Cliente,
        int Origen
    ) : INotification;
}
