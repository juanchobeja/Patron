using Core.Abstractions;
using Core.Entities;
using Dapper;
using Infrastructure.Connection;

namespace Infrastructure.Repositories
{
    public class FileTrazabilidadRepository : BaseRepository<FileTrazabilidad>, IFileTrazabilidadRepository
    {
        public FileTrazabilidadRepository(ISqlConnectionFactory factory) : base(factory) { }

        public override async Task AddAsync(FileTrazabilidad entity, CancellationToken ct = default)
        {
            const string sql = @"
                INSERT INTO Trazabilidad
                (NombreArchivoOriginal, NombreArchivoGenerado, FechaGeneración, NombreGuane, Cliente, Origen)
                VALUES (@NombreArchivoOriginal,@NombreArchivoGenerado,@FechaGeneracion,@NombreGuane,@Cliente,@Origen)";

            await ExecuteInsertAsync(sql, entity, ct);
        }

        public async Task UpdateNombreGuaneAsync(Guid nombreOriginal, Guid guaneId, CancellationToken ct = default)
        {
            const string sql = @"
                UPDATE Trazabilidad
                SET NombreGuane = @guaneId
                WHERE NombreArchivoGenerado = @nombreOriginal";

            await _tx.Connection.ExecuteAsync(sql, new { guaneId, nombreOriginal }, _tx);
        }

        public async Task<FileTrazabilidad?> GetByNombreArchivoGeneradoAsync(Guid archivoGenerado, CancellationToken ct = default)
        {
            try
            {
                const string sql = @"
                SELECT Id, NombreArchivoOriginal, NombreArchivoGenerado, FechaGeneración AS FechaGeneracion, NombreGuane, Cliente, Origen
                FROM Trazabilidad
                WHERE NombreGuane = @archivoGenerado";

                return await _tx.Connection.QuerySingleOrDefaultAsync<FileTrazabilidad>(sql, new { archivoGenerado }, _tx);
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }

    }
}
