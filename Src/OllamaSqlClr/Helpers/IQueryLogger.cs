namespace OllamaSqlClr.Helpers
{
    public interface IQueryLogger
    {
        void LogQuerySuccess(string prompt, string query);
        void LogQueryError(string prompt, string query, string errorNumber, string errorMessage, string errorLine);
        void LogQueryExecution(string prompt, string query, string errorNumber, string errorMessage, string errorLine);
    }
}

