using System.Data;
using System.Data.SqlClient;

namespace OllamaSqlClr.DataAccess
{
    public class DatabaseExecutor : IDatabaseExecutor
    {
        private readonly SqlConnection _connection;

        public DatabaseExecutor()
        {
            // Use the context connection to connect to the SQL Server hosting the CLR
            _connection = new SqlConnection("context connection=true");
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

