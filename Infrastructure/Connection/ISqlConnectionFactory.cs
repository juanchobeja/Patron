using Microsoft.Data.SqlClient;

namespace Infrastructure.Connection
{
    public interface ISqlConnectionFactory
    {
        SqlConnection CreateConnection();
    }
}
