using System.Text.RegularExpressions;

namespace OllamaSqlClr.Helpers
{
    // Check for SQL keywords that alter data
    public class QueryValidator
    {
        public bool IsSafeQuery(string query)
        {
            string unsafeKeywordsPattern = @"\b(INSERT|UPDATE|DELETE|DROP|ALTER|TRUNCATE|EXEC|EXECUTE|CREATE|GRANT|REVOKE|DENY)\b|no reply";
            return !Regex.IsMatch(query, unsafeKeywordsPattern, RegexOptions.IgnoreCase);
        }

        // Additional validation methods as needed
    }
}
