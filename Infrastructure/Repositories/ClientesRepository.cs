using Core.Abstractions;
using Core.Entities;
using Dapper;
using Infrastructure.Connection;

namespace Infrastructure.Repositories
{
    public class ClientesRepository : IClientesRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public ClientesRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Cliente>> GetClientesActivosAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
            SELECT 
                id, nombre, 
                RutaOrigenSftp, RutaOrigenEmail, RutaOrigenMasivos,RutaOrigenSftpGuane,RutaOrigenEmailGuane,RutaIntegracionMasivos,
                RutaProcesadosGuaneSftp, RutaProcesadosGuaneEmail,
                RutaIntegracionMasivos, RutaError,RutaAlmacenada
            FROM Clientes
            WHERE Activo = 1";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<Cliente>(sql);
        }
    }
}
