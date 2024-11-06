using OllamaSqlClr.DataAccess;
using System;
using System.Data.SqlClient;
using System.Data;

namespace OllamaSqlClr.Helpers
{
    // Handle SQL commands

    public class SqlCommand : ISqlCommand
    {
        public readonly IDatabaseExecutor _dbExecutor;

        public SqlCommand(IDatabaseExecutor dbExecutor)
        {
            _dbExecutor = dbExecutor;
        }

        public string CreateProcedureFromQuery(string query)
        {
            string limitedQuery = $@"
                SELECT TOP 100 * FROM ({query}) AS LimitedResult";

            string wrappedQuery = $@"
                BEGIN TRY
                    {limitedQuery}
                END TRY
                BEGIN CATCH
                    SELECT 
                        ERROR_NUMBER() AS ErrorNumber,
                        ERROR_MESSAGE() AS ErrorMessage,
                        ERROR_LINE() AS ErrorLine;
                END CATCH";

            string name = "#TempProc_" + Guid.NewGuid().ToString("N");

            string createProcedureCommand = $@"
                CREATE PROCEDURE {name}
                AS
                BEGIN
                    SET NOCOUNT ON;
                    {wrappedQuery}
                END";

            _dbExecutor.ExecuteNonQuery(createProcedureCommand);

            return name;
        }

        public DataTable RunTemporaryProcedure(string name)
        {
            DataTable resultTable = new DataTable();
            string executeProcedureCommand = $"EXEC {name};";

            using (var cmd = new System.Data.SqlClient.SqlCommand(executeProcedureCommand, _dbExecutor.GetConnection()))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                adapter.Fill(resultTable);
            }
            return resultTable;
        }

        public (bool, string) DropTemporaryProcedure(string name)
        {
            string dropProcedureCommand = $"DROP PROCEDURE {name}";

            try
            {
                _dbExecutor.ExecuteNonQuery(dropProcedureCommand);

                return (true, $"Procedure {name} was dropped successfully.");
            }
            catch (Exception ex)
            {
                string message = $"Error: Failed to drop procedure {name}: {ex.Message}";
                return (false, message);
            }
        }
    }
}
