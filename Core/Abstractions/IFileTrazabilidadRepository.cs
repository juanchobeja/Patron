using Core.Entities;

namespace Core.Abstractions
{
    public interface IFileTrazabilidadRepository : IRepository<FileTrazabilidad>
    {
        Task UpdateNombreGuaneAsync(Guid nombreOriginal, Guid guaneId, CancellationToken ct = default);
        Task<FileTrazabilidad?> GetByNombreArchivoGeneradoAsync(Guid archivoGenerado, CancellationToken ct = default);
    }
}
