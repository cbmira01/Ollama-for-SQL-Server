using System.Text.RegularExpressions;

namespace OllamaSqlClr.Helpers
{
    public class QueryValidator : IQueryValidator
    {
        public bool IsUnsafe(string query)
        {
            string unsafeKeywordsPattern = @"\b(INSERT|UPDATE|DELETE|DROP|ALTER|TRUNCATE|EXEC|EXECUTE|CREATE|GRANT|REVOKE|DENY)\b";
            return Regex.IsMatch(query, unsafeKeywordsPattern, RegexOptions.IgnoreCase);
        }

        public bool IsNoReply(string query)
        {
            return Regex.IsMatch(query, @"\bno reply\b", RegexOptions.IgnoreCase);
        }

        public bool IsRejected(string query)
        {
            return Regex.IsMatch(query, @"rejected", RegexOptions.IgnoreCase);
        }

        public bool IsNullOrEmpty(string query) 
        {
            if (string.IsNullOrEmpty(query))
            {
                return true;
            }
            return false;
        }

        public string CleanQuery(string dirtyQuery)
        {
            if (string.IsNullOrEmpty(dirtyQuery))
            {
                return dirtyQuery;
            }

            // Replace '003c' (hex for '<') with the '<' symbol
            string cleanQuery = Regex.Replace(dirtyQuery, @"003c", "<", RegexOptions.IgnoreCase);

            // Replace '003e' (hex for '>') with the '>' symbol
            cleanQuery = Regex.Replace(cleanQuery, @"003e", ">", RegexOptions.IgnoreCase);

            // Clean up code bracketing
            cleanQuery = Regex.Replace(cleanQuery, @"```sql", " ", RegexOptions.IgnoreCase);
            cleanQuery = Regex.Replace(cleanQuery, @"```.*?$", " ", RegexOptions.Multiline);
            cleanQuery = Regex.Replace(cleanQuery, @"`", "", RegexOptions.IgnoreCase);

            // Clean up trailing comments
            cleanQuery = Regex.Replace(cleanQuery, @"--.*?$", "", RegexOptions.Multiline);

            // Clean up newlines
            cleanQuery = Regex.Replace(cleanQuery, @"\n", " ", RegexOptions.IgnoreCase);

            return cleanQuery;
        }
    }
}
