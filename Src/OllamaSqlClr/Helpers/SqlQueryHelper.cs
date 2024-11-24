using OllamaSqlClr.DataAccess;
using System;
using System.Data;
using System.Data.SqlClient;

namespace OllamaSqlClr.Helpers
{
    public class SqlQueryHelper : ISqlQueryHelper, IDisposable
    {
        private readonly IDatabaseExecutor _dbExecutor;
        private bool _isDdisposed;

        public SqlQueryHelper(IDatabaseExecutor dbExecutor)
        {
            _dbExecutor = dbExecutor ?? throw new ArgumentNullException(nameof(dbExecutor));
        }

        public DataTable ExecuteProcedure(string procedureName)
        {
            if (string.IsNullOrEmpty(procedureName))
                throw new ArgumentException("Procedure name cannot be null or empty.", nameof(procedureName));

            DataTable resultTable = new DataTable();
            string executeProcedureCommand = $"EXEC {procedureName};";

            using (var connection = _dbExecutor.GetConnection())
            using (var command = new SqlCommand(executeProcedureCommand, connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                adapter.Fill(resultTable);
            }

            return resultTable;
        }

        public void Dispose()
        {
            if (!_isDdisposed)
            {
                if (_dbExecutor is IDisposable disposableExecutor)
                {
                    disposableExecutor.Dispose();
                }
                _isDdisposed = true;
            }
        }
    }
}
