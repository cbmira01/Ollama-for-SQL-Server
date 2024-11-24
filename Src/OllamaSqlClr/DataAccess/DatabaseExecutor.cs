using OllamaSqlClr.DataAccess;
using System.Data.SqlClient;
using System.Data;
using System;

public class DatabaseExecutor : IDatabaseExecutor
{
    private readonly string _connectionString;

    public DatabaseExecutor(string connectionString = "context connection=true")
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
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
}
