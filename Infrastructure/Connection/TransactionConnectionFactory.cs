using Microsoft.Data.SqlClient;

namespace Infrastructure.Connection
{
    public class TransactionConnectionFactory : ISqlConnectionFactory
    {
        public SqlConnection Connection { get; }
        public SqlTransaction Transaction { get; }

        public TransactionConnectionFactory(SqlConnection connection, SqlTransaction transaction)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        public SqlConnection CreateConnection() => Connection;
    }
}
