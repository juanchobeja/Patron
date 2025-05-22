using Core.Entities;

namespace Core.Abstractions
{
    public interface IFormatFileRepository
    {
        Task<FormatFile?> GetByNombreOriginalAsync(Guid nombreOriginal, CancellationToken ct = default);
    }
}
