namespace Core.Entities
{
    public record Solicitud(
        Guid BatchId,
        Guid SolicitudId,
        string Etiqueta,
        bool Masivo,
        string NombreArchivoOriginal
    );
}
