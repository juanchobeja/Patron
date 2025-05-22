using Core.Abstractions;
using Core.Entities;
using Dapper;
using Infrastructure.Connection;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories
{
    public class ConsecutivoEtiquetaRepository : IConsecutivoEtiquetaRepository
    {
        private readonly SqlTransaction _tx;

        public ConsecutivoEtiquetaRepository(ISqlConnectionFactory factory)
        {
            if (factory is TransactionConnectionFactory tcf)
                _tx = tcf.Transaction;
            else
                throw new InvalidOperationException("Se requiere una transacción activa.");
        }

        public async Task<IQEtiqueta> GetAndIncrementAsync(string cliente, CancellationToken ct = default)
        {
            const string sel = @"
            SELECT                     
                Codigo,
                Etiqueta,                    
                Longitud,
                Condicion,
                TRY_CAST(Consecutivo AS BIGINT) AS Consecutivo,
                ConsecutivoEtiqueta,
                SeparacionCliente
            FROM ConsecutivosEtiquetas WITH (ROWLOCK, UPDLOCK)
            WHERE Codigo = @cliente AND SeparacionCliente = 0";

            const string upd = @"
            UPDATE ConsecutivosEtiquetas
            SET Consecutivo = @Consecutivo,
                ConsecutivoEtiqueta = @ConsecutivoEtiqueta
            WHERE Codigo = @Codigo
                AND SeparacionCliente = 0";

            const int maxRetries = 3;

            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    var ce = await _tx.Connection.QuerySingleAsync<IQEtiqueta>(
                        sel,
                        new { cliente },
                        transaction: _tx,
                        commandTimeout: 60);

                    // Generar nuevo consecutivo y etiqueta
                    var nuevoConsecutivo = ce.Consecutivo + 1;
                    var etiquetaGenerada = ce with { Consecutivo = nuevoConsecutivo };
                    var etiquetaFinal = etiquetaGenerada.GenerarEtiqueta();

                    await _tx.Connection.ExecuteAsync(
                        upd,
                        new
                        {
                            Codigo = etiquetaGenerada.Codigo,
                            Consecutivo = nuevoConsecutivo,
                            ConsecutivoEtiqueta = etiquetaFinal
                        },
                        transaction: _tx,
                        commandTimeout: 60);

                    return etiquetaGenerada with { ConsecutivoEtiqueta = etiquetaFinal };
                }
                catch (SqlException ex) when (ex.Number == -2 && attempt < maxRetries)
                {
                    await Task.Delay(200 * attempt, ct);
                }
            }
        }
    }


}
