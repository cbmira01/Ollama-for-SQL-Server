using OllamaSqlClr.DataAccess;
using System.Data.SqlClient;
using System.Data;
using System;
using System.Security.Cryptography;

public class DatabaseExecutor : IDatabaseExecutor
{
    public string ConnectionString { get; }

    public DatabaseExecutor(string connectionString = "context connection=true")
    {
        ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public DataTable ExecuteQuery(string query)
    {
        using (var connection = new SqlConnection(ConnectionString))
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

        using (var connection = new SqlConnection(ConnectionString))
        using (var cmd = new SqlCommand(commandText, connection))
        {
            connection.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public void ExecuteNonQuery(string commandText, params SqlParameter[] parameters)
    {
        using (var connection = new SqlConnection(ConnectionString))
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

    public void ExecuteNonQuery(string commandText, SqlTransaction transaction, params SqlParameter[] parameters)
    {
        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction), "Transaction cannot be null when executing with a transaction.");
        }

        using (var cmd = new SqlCommand(commandText, transaction.Connection, transaction))
        {
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }

            cmd.ExecuteNonQuery();
        }
    }
}
