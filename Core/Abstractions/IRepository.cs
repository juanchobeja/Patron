
namespace Core.Abstractions
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task AddAsync(TEntity entity, CancellationToken ct = default);
        
    }
}
