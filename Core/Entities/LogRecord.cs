namespace Core.Entities
{
    public record LogRecord(
        long Id,
        DateTime FechaEvento,
        string Archivo,
        string Mensaje,
        string Servicio,
        string Level,
        string? Exception = null,
        Guid? CorrelationId = null
    );
}

