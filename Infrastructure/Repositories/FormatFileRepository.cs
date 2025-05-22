using Core.Abstractions;
using Core.Entities;
using Dapper;
using Infrastructure.Connection;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories
{
    public class FormatFileRepository : IFormatFileRepository
    {
        private readonly SqlTransaction _tx;
        public FormatFileRepository(ISqlConnectionFactory factory)
        {
            if (factory is TransactionConnectionFactory tcf) _tx = tcf.Transaction;
        }
        public async Task<FormatFile?> GetByNombreOriginalAsync(Guid nombreOriginal, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT GuaneId, NombreOriginal
                FROM Format_File
                WHERE NombreOriginal = @nombreOriginal";
            return await _tx.Connection.QuerySingleOrDefaultAsync<FormatFile>(
                sql, new { nombreOriginal }, transaction: _tx);
        }
    }
}
