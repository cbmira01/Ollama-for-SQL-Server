namespace OllamaSqlClr.Helpers
{
    public interface IQueryValidator
    {
        bool IsSafeQuery(string query);

        bool IsNoReply(string query);
    }
}

