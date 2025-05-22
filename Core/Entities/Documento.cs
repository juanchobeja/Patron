namespace Core.Entities
{
    public record Documento(
        Guid DocumentoId,
        Guid BatchId,
        Guid FolderId,
        string RutaImagen,
        string FullPath
    );
}
