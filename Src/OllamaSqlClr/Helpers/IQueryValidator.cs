namespace OllamaSqlClr.Helpers
{
    public interface IQueryValidator
    {
        bool IsSafeQuery(string query);
    }
}

