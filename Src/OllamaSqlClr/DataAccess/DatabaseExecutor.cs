using System.Data;
using System.Data.SqlClient;

namespace OllamaSqlClr.DataAccess
{
    public class DatabaseExecutor : IDatabaseExecutor
    {
        private readonly SqlConnection _connection;

        // TODO: This is wrong, connection string should come from either testing or TSQL configuration
        public DatabaseExecutor(string connectionString = "context connection=true")
        {
            _connection = new SqlConnection(connectionString);
            _connection.Open();
        }

        public DataTable ExecuteQuery(string query)
        {
            using (var cmd = new SqlCommand(query, _connection))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                var resultTable = new DataTable();
                adapter.Fill(resultTable);
                return resultTable;
            }
        }

        public void ExecuteNonQuery(string commandText)
        {
            using (var cmd = new SqlCommand(commandText, _connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public SqlConnection GetConnection() => _connection; // Return the context connection
    }
}

