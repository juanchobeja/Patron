namespace Core.Entities
{
    public record Batch(
        Guid BatchId,
        long Consecutivo,
        int OrigenImagen,
        DateTime FechaProceso
    );
}
