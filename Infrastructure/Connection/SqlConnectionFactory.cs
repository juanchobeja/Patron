using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Connection
{
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public SqlConnection CreateConnection()
        {
            string existingConnectionString = _configuration.GetConnectionString("ConexionBD")!;
            SqlConnectionStringBuilder strbldr = new SqlConnectionStringBuilder(existingConnectionString);
            strbldr.DataSource = strbldr.DataSource;
            strbldr.InitialCatalog = strbldr.InitialCatalog;
            strbldr.IntegratedSecurity = strbldr.IntegratedSecurity;
            strbldr.ColumnEncryptionSetting = strbldr.ColumnEncryptionSetting;
            //strbldr.ColumnEncryptionSetting= SqlConnectionColumnEncryptionSetting.Enabled;
            SqlConnection connection = new SqlConnection(strbldr.ConnectionString);
            return connection;

        }
    }


}
