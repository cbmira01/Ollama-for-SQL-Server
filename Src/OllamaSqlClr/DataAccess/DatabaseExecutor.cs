using System.Data.SqlClient;
using System.Data;
using System;
using Configuration;

namespace OllamaSqlClr.DataAccess
{
    public class DatabaseExecutor : IDatabaseExecutor
    {
        private string _connectionString = AppConfig.SqlClrContextConnection;

        public DatabaseExecutor()
        {
        }

        public DataTable ExecuteQuery(string query)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, connection))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                connection.Open();
                var resultTable = new DataTable();
                adapter.Fill(resultTable);
                return resultTable;
            }
        }

        public void ExecuteNonQuery(string commandText)
        {

            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(commandText, connection))
            {
                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void ExecuteNonQuery(string commandText, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(commandText, connection))
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool TryQuery(string proposedQuery)
        {
            try
            {
                ExecuteNonQuery(proposedQuery);
                return true;
            }
            catch (SqlException ex)
            {
                // var _ = ex;
                Console.WriteLine($"Query trial failed: {ex.Message}");
                return false;
            }
        }
    }
}