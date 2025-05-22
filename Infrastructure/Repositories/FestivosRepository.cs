using Core.Abstractions;
using Dapper;
using Infrastructure.Connection;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class FestivosRepository : IFestivosRepository
    {
        private readonly SqlTransaction _tx;

        public FestivosRepository(ISqlConnectionFactory factory)
        {
            if (factory is TransactionConnectionFactory tcf)
                _tx = tcf.Transaction;
            else
                throw new InvalidOperationException("Se requiere TransactionConnectionFactory");
        }

        public async Task<DateTime> ObtenerSiguienteDiaHabilAsync(DateTime desdeFecha, CancellationToken ct = default)
        {
            const string sql = @"
            SELECT TOP 1 Fecha
            FROM Festivos
            WHERE Fecha > @desdeFecha
              AND DiaHabil = 1 AND Festivo = 0
            ORDER BY Fecha ASC";

            var resultado = await _tx.Connection.QuerySingleOrDefaultAsync<DateTime?>(sql, new { desdeFecha }, _tx);
            return resultado ?? desdeFecha.AddDays(1); 
        }
    }

}
