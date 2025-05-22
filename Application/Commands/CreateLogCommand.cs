using Application.Abstractions.Messaging;

namespace Application.Commands
{
    public record CreateLogCommand(
        string Archivo,
        string Mensaje,
        string Servicio,
        string Level,
        string? Exception = null,
        Guid? CorrelationId = null
    ) : ICommand;
}
