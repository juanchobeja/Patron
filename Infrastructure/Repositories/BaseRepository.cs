using Core.Abstractions;
using Dapper;
using Infrastructure.Connection;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories
{
    public abstract class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly SqlTransaction _tx;
        protected readonly SqlConnection _conn;

        protected BaseRepository(ISqlConnectionFactory factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            if (factory is TransactionConnectionFactory tcf)
            {
                _tx = tcf.Transaction;
                _conn = tcf.Connection;
            }
            else
            {
                _conn = factory.CreateConnection();
                _conn.Open();
            }
        }

        public abstract Task AddAsync(TEntity entity, CancellationToken ct = default);

        protected async Task ExecuteInsertAsync(string sql, object parameters, CancellationToken ct = default)
        {
            if (_tx != null)
            {
                await _tx.Connection.ExecuteAsync(sql, parameters, _tx);
            }
            else
            {
                await _conn.ExecuteAsync(sql, parameters);
            }
        }

        public void Dispose()
        {
            if (_tx == null) 
            {
                _conn?.Close();
                _conn?.Dispose();
            }
        }
    }
}
