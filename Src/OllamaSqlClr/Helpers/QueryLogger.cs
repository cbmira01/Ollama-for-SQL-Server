using OllamaSqlClr.DataAccess;
using System;

namespace OllamaSqlClr.Helpers
{
    // Logger class for logging SQL queries and errors
    public class QueryLogger : IQueryLogger
    {
        private readonly IDatabaseExecutor _dbExecutor;

        public QueryLogger(IDatabaseExecutor dbExecutor)
        {
            _dbExecutor = dbExecutor;
        }

        public void LogQuerySuccess(string prompt, string query)
        {
            LogQueryExecution(prompt, query, null, null, null);
        }

        public void LogQueryError(string prompt, string query, string errorNumber, string errorMessage, string errorLine)
        {
            LogQueryExecution(prompt, query, errorNumber, errorMessage, errorLine);
        }

        public void LogQueryExecution(string prompt, string query, string errorNumber, string errorMessage, string errorLine)
        {
            string logQueryCommand = @"
                INSERT INTO QueryPromptLog (Prompt, Query, ErrorNumber, ErrorMessage, ErrorLine) 
                VALUES (@Prompt, @Query, @ErrorNumber, @ErrorMessage, @ErrorLine);";

            using (var cmd = new System.Data.SqlClient.SqlCommand(logQueryCommand, _dbExecutor.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@Prompt", (object)prompt ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Query", (object)query ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ErrorNumber", (object)errorNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ErrorMessage", (object)errorMessage ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ErrorLine", (object)errorLine ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
