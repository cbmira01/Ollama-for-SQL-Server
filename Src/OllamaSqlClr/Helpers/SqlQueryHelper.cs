using OllamaSqlClr.DataAccess;
using System;
using System.Data;

namespace OllamaSqlClr.Helpers
{
    public class SqlQueryHelper : ISqlQueryHelper
    {
        private readonly IDatabaseExecutor _dbExecutor;

        public SqlQueryHelper(IDatabaseExecutor dbExecutor)
        {
            _dbExecutor = dbExecutor ?? throw new ArgumentNullException(nameof(dbExecutor));
        }

        public DataTable ExecuteProcedure(string procedureName)
        {
            if (string.IsNullOrEmpty(procedureName))
            {
                throw new ArgumentException("Procedure name cannot be null or empty.", nameof(procedureName));
            }

            string executeProcedureCommand = $"EXEC {procedureName};";
            return _dbExecutor.ExecuteQuery(executeProcedureCommand);
        }
    }
}
