namespace Core.Entities
{
    public record FileTrazabilidad(
        int Id,
        string NombreArchivoOriginal,
        Guid NombreArchivoGenerado,
        DateTime FechaGeneracion,
        Guid? NombreGuane,
        string Cliente,
        int Origen
    );
}
