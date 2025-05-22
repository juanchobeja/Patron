using Core.Abstractions;
using Dapper;
using Infrastructure.Connection;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories
{
    public class ConfiguradorIntegradorRepository : IConfiguradorIntegradorRepository
    {
        private readonly SqlTransaction _tx;

        public ConfiguradorIntegradorRepository(ISqlConnectionFactory factory)
        {
            if (factory is TransactionConnectionFactory tcf)
                _tx = tcf.Transaction;
            else
                throw new InvalidOperationException(
                    "ConfiguradorIntegradorRepository requires TransactionConnectionFactory");
        }

        public async Task<long> GetAndIncrementBatchConsecutiveAsync(CancellationToken ct = default)
        {
            const string selectSql = @"
            SELECT valor
            FROM configuradorIntegrador WITH (ROWLOCK, UPDLOCK)
            WHERE campos = 'consecutivoBatch' AND activo = 1";

            const string updateSql = @"
            UPDATE configuradorIntegrador
            SET valor = @newValue
            WHERE campos = 'consecutivoBatch' AND activo = 1";

            const int maxRetries = 3;

            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    var valorActual = await _tx.Connection.QuerySingleOrDefaultAsync<string>(
                        selectSql,
                        transaction: _tx,
                        commandTimeout: 60);

                    if (string.IsNullOrWhiteSpace(valorActual) || !long.TryParse(valorActual, out var current))
                        throw new InvalidOperationException("El valor actual del consecutivo no es válido.");

                    var next = current + 1;

                    await _tx.Connection.ExecuteAsync(
                        updateSql,
                        new { newValue = next.ToString() },
                        transaction: _tx,
                        commandTimeout: 60);

                    return next;
                }
                catch (SqlException ex) when (ex.Number == -2 && attempt < maxRetries)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), ct);
                    continue;
                }
            }
        }

        public async Task<int> GetHoraConfiguradaAsync(string campo, CancellationToken ct = default)
        {
            const string sql = @"
        SELECT valor 
        FROM configuradorIntegrador 
        WHERE campos = @campo AND activo = 1";

            var valor = await _tx.Connection.QuerySingleOrDefaultAsync<string>(sql, new { campo }, _tx);
            if (!int.TryParse(valor, out var hora))
                throw new InvalidOperationException($"El valor de '{campo}' no es un número de hora válido.");

            return hora;
        }


    }
}
