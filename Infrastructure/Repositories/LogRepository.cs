using Core.Abstractions;
using Core.Entities;
using Dapper;
using Infrastructure.Connection;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure.Repositories
{
    public class LogRepository : ILogRepository
    {
        private readonly SqlTransaction _tx;
        public LogRepository(ISqlConnectionFactory factory)
        {
            if (factory is TransactionConnectionFactory tcf) _tx = tcf.Transaction;
        }
        public async Task AddAsync(LogRecord log)
        {
            const string sql = @"
                INSERT INTO ws_Integracion
                (FechaEvento, Archivo, Mensaje, Servicio, Level, Exception, CorrelationId)
                VALUES
                (@FechaEvento,@Archivo,@Mensaje,@Servicio,@Level,@Exception,@CorrelationId)";
            await _tx.Connection.ExecuteAsync(sql, log, _tx);
        }


        public async Task<IEnumerable<LogRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            const string sql = @"
                SELECT Id, FechaEvento, Archivo, Mensaje, Servicio, Level, Exception, CorrelationId
                FROM ws_Integracion
                WHERE FechaEvento BETWEEN @StartDate AND @EndDate";

            return await _tx.Connection.QueryAsync<LogRecord>(
                sql,
                new { StartDate = startDate, EndDate = endDate },
                transaction: _tx,
                commandType: CommandType.Text
            );
        }

    }
}