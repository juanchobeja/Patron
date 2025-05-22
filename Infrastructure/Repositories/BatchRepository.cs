using Core.Entities;
using Infrastructure.Connection;

namespace Infrastructure.Repositories
{
    public class BatchRepository : BaseRepository<Batch>
    {
        public BatchRepository(ISqlConnectionFactory factory) : base(factory) { }

        public override async Task AddAsync(Batch batch, CancellationToken ct = default)
        {
            const string sql = @"
                INSERT INTO Batch(Batch_Id, Consecutivo, OrigenImagen, FechaProceso)
                VALUES(@BatchId, @Consecutivo, @OrigenImagen, @FechaProceso)";
            await ExecuteInsertAsync(sql, batch, ct);
        }
    }
}
