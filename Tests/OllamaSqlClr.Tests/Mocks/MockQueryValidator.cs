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

        public bool IsNullOrEmpty(string query)
        {
            if (query == "mockNoReply")
            {
                return false;
            }

            return true;
        }
    }
}
