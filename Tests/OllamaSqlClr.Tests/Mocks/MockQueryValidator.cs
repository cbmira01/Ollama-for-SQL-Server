using OllamaSqlClr.Helpers;

namespace OllamaSqlClr.Tests.Mocks
{
    public class MockQueryValidator : IQueryValidator
    {
        public bool IsUnsafe(string query)
        {
            if (query == "mockUnsafeQuery")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsNoReply(string query)
        {
            if (query == "mockNoReply")
            {
                return true;
            }
            else 
            {
                return false;
            }
        }

        public bool IsRejected(string query)
        {
            if (query == "mockRejected")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsNullOrEmpty(string query)
        {
            if (query == "mockNullOrEmpty")
            {
                return true;
            }

            return false;
        }

        public string CleanQuery(string query)
        {
            if (query == "mockNullOrEmpty")
            {
                return "mockNullOrEmpty";
            }

            return "mockCleanQuery";
        }
    }
}
