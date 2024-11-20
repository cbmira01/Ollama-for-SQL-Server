using OllamaSqlClr.DataAccess;
using OllamaSqlClr.Helpers;
using System.Data;
using System.Data.SqlClient;

namespace OllamaSqlClr.Tests.Mocks
{
    public class MockSqlQuery : ISqlQuery
    {
        public readonly IDatabaseExecutor _dbExecutor;

        public MockSqlQuery(IDatabaseExecutor dbExecutor)
        {
            _dbExecutor = dbExecutor;
        }

        public DataTable ExecuteProcedure(string name)
        {
            // Return a mock DataTable
            var table = new DataTable();
            table.Columns.Add("ColumnMockSqlQuery");
            table.Rows.Add("Mocked MockSqlQuery Row 1");
            table.Rows.Add("Mocked MockSqlQuery Row 2");
            return table;
        }
    }
}
