using OllamaSqlClr.DataAccess;
using System;

namespace OllamaSqlClr.Helpers
{
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
            string logQueryCommand = $@"
                INSERT INTO QueryPromptLog (Prompt, Query, ErrorNumber, ErrorMessage, ErrorLine)
                VALUES ('{prompt}', '{query}', '{errorNumber}', '{errorMessage}', '{errorLine}');";

            // Use the injected database executor to "execute" the SQL
            _dbExecutor.ExecuteNonQuery(logQueryCommand);
        }
    }
}
