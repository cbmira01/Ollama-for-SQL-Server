using OllamaSqlClr.DataAccess;
using OllamaSqlClr.Helpers;
using System;
using System.Data;

namespace OllamaSqlClr.Tests.Mocks
{
    public class MockSqlCommandHelper : ISqlCommandHelper
    {
        public readonly IDatabaseExecutor _dbExecutor;

        public MockSqlCommandHelper(IDatabaseExecutor dbExecutor)
        {
            _dbExecutor = dbExecutor;
        }

        public string CreateProcedureFromQuery(string query)
        {
            return $"Mock procedure created from \"{query}\" ";
        }

        public DataTable RunProcedure(string name)
        {
            // Return a mock DataTable
            var table = new DataTable();
            table.Columns.Add("ColumnMockSqlCommandHelper");
            table.Rows.Add("Mocked MockSqlCommand Row 1");
            table.Rows.Add("Mocked MockSqlCommand Row 2");
            return table;
        }

        public (bool, string) DropProcedure(string name)
        {
            return (true, $"Mocked procedure \"{name}\" was dropped successfully.");
        }
    }
}
