using OllamaSqlClr.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace OllamaSqlClr.Tests.Helpers
{
    public class FakeDatabaseExecutor : IDatabaseExecutor
    {
        // List to track executed SQL queries
        public List<string> ExecutedQueries { get; } = new List<string>();

        public DataTable ExecuteQuery(string query)
        {
            ExecutedQueries.Add(query);
            return new DataTable(); // Return an empty DataTable as a stub
        }

        public void ExecuteNonQuery(string commandText)
        {
            ExecutedQueries.Add(commandText);
        }

        public SqlConnection GetConnection()
        {
            throw new NotImplementedException("GetConnection is not used in this test setup.");
        }
    }
}
