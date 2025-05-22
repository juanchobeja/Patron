using Core.Entities;

namespace Core.Abstractions
{
    public interface IConsecutivoEtiquetaRepository
    {
        Task<IQEtiqueta> GetAndIncrementAsync(string cliente, CancellationToken ct = default);
    }
}
