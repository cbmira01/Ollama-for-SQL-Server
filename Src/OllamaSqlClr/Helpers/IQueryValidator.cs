namespace OllamaSqlClr.Helpers
{
    public interface IQueryValidator
    {
        bool IsUnsafe(string query);

        bool IsNoReply(string query);

        bool IsRejected(string query);

        bool IsNullOrEmpty(string query);

        string CleanQuery(string query);
    }
}

