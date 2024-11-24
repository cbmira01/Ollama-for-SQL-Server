using OllamaSqlClr.DataAccess;
using System;
using System.Data;
using System.Data.SqlClient;

namespace OllamaSqlClr.Helpers
{
    public class SqlCommandHelper : ISqlCommandHelper, IDisposable
    {
        private readonly IDatabaseExecutor _dbExecutor;
        private bool _isDdisposed;

        public SqlCommandHelper(IDatabaseExecutor dbExecutor)
        {
            _dbExecutor = dbExecutor ?? throw new ArgumentNullException(nameof(dbExecutor));
        }

        public string CreateProcedureFromQuery(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));
            }

            string name = "#TempProc_" + Guid.NewGuid().ToString("N");

            string createProcedureCommand = $@"
                CREATE PROCEDURE {name}
                AS
                BEGIN
                    SET NOCOUNT ON;
                    BEGIN TRY
                        SELECT TOP 100 * FROM ({query}) AS LimitedResult;
                    END TRY
                    BEGIN CATCH
                        SELECT 
                            ERROR_NUMBER() AS ErrorNumber,
                            ERROR_MESSAGE() AS ErrorMessage,
                            ERROR_LINE() AS ErrorLine;
                    END CATCH
                END";

            _dbExecutor.ExecuteNonQuery(createProcedureCommand);

            return name;
        }

        public DataTable RunProcedure(string procedureName)
        {
            if (string.IsNullOrEmpty(procedureName))
            {
                throw new ArgumentException("Procedure name cannot be null or empty.", nameof(procedureName));
            }

            string executeProcedureCommand = $"EXEC {procedureName};";
            DataTable resultTable = new DataTable();

            using (var connection = _dbExecutor.GetConnection())
            using (var command = new SqlCommand(executeProcedureCommand, connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                adapter.Fill(resultTable);
            }

            return resultTable;
        }

        public (bool success, string message) DropProcedure(string procedureName)
        {
            if (string.IsNullOrEmpty(procedureName))
            {
                throw new ArgumentException("Procedure name cannot be null or empty.", nameof(procedureName));
            }

            string dropProcedureCommand = $"DROP PROCEDURE {procedureName}";

            try
            {
                _dbExecutor.ExecuteNonQuery(dropProcedureCommand);
                return (true, $"Procedure {procedureName} was dropped successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: Failed to drop procedure {procedureName}: {ex.Message}");
            }
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
