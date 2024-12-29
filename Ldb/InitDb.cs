using System;
using System.Data.SqlClient;
using System.IO;

namespace Ldb
{
    public class InitDb
    {
        private readonly SqlConnection _connection;

        public InitDb()
        {
            // Inicializar la conexión utilizando la cadena de conexión de ConfigConnection
            _connection = new SqlConnection(DbConfig.ConnectionString);
        }

        public void InitializeDatabase()
        {
            if (!DatabaseExists())
            {
                ExecuteScript("Sentences.sql");
            }
        }

        private bool DatabaseExists()
        {
            using (var command = new SqlCommand("SELECT database_id FROM sys.databases WHERE name = 'LocalChurch'", _connection))
            {
                _connection.Open();
                var exists = command.ExecuteScalar() != null;
                _connection.Close();
                return exists;
            }
        }

        private void ExecuteScript(string scriptPath)
        {
            string script = File.ReadAllText(scriptPath);

            using (var command = new SqlCommand(script, _connection))
            {
                _connection.Open();
                command.ExecuteNonQuery();
                _connection.Close();
            }
        }
    }
}
