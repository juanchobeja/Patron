using Core.Entities;

namespace Core.Abstractions
{
    public interface IClientesRepository
    {
        Task<IEnumerable<Cliente>> GetClientesActivosAsync(CancellationToken cancellationToken = default);
    }
}
