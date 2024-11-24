using System;
using System.Text.RegularExpressions;

namespace OllamaSqlClr.Helpers
{
    // Check for SQL keywords that alter data
    public class QueryValidator : IQueryValidator
    {
        public bool IsSafeQuery(string query)
        {
            string unsafeKeywordsPattern = @"\b(INSERT|UPDATE|DELETE|DROP|ALTER|TRUNCATE|EXEC|EXECUTE|CREATE|GRANT|REVOKE|DENY)\b";
            return !Regex.IsMatch(query, unsafeKeywordsPattern, RegexOptions.IgnoreCase);
        }

        public bool IsNoReply(string query)
        {
            return Regex.IsMatch(query, @"\bno reply\b", RegexOptions.IgnoreCase);
        }

        public bool IsNullOrEmpty(string query) 
        {
            if (string.IsNullOrEmpty(query))
            {
                return true;
            }
            return false;
        }
    }
}
