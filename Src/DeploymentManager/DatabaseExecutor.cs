using System;
using System.Data.SqlClient;

namespace DeploymentManager
{
    public class DatabaseExecutor
    {
        private readonly string _connectionString;

        public DatabaseExecutor(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void ExecuteSql(string sqlScript)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(sqlScript, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
