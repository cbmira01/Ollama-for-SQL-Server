using OllamaSqlClr.DataAccess;
using System.Data;
using System.Data.SqlClient;

namespace OllamaSqlClr.Tests.Mocks
{
    public class MockDatabaseExecutor : IDatabaseExecutor
    {
        public DataTable ExecuteQuery(string query)
        {
            // Return a mock DataTable
            var table = new DataTable();
            table.Columns.Add("ColumnMockDatabaseExecutor");
            table.Rows.Add("Mocked MockDatabaseExecutor Row 1");
            table.Rows.Add("Mocked MockDatabaseExecutor Row 2");
            return table;
        }

        public void ExecuteNonQuery(string commandText)
        {
            // No-op for testing
        }

        public SqlConnection GetConnection()
        {
            return null; // No connection needed in test context
        }
    }
}

