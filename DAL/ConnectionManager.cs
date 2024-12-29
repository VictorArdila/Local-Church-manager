using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace DAL
{
    public class ConnectionManager
    {
        public SqlConnection _conexion;
        public ConnectionManager(string connectionstring)
        {
            _conexion = new SqlConnection(connectionstring);
        }
        public void Open()
        {
            _conexion.Open();
        }
        public void Close()
        {
            _conexion.Close();
        }
    }
}
