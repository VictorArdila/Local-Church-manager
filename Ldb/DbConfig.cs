using System.Configuration;
using System.Data.SqlClient;

namespace Ldb
{
    public static class DbConfig
    {
        public static string ConnectionString = ConfigurationManager.ConnectionStrings["conexion"].ConnectionString;
        public static string ProviderName = ConfigurationManager.ConnectionStrings["conexion"].ProviderName;
    }
}
