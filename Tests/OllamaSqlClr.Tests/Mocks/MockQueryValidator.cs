using OllamaSqlClr.Helpers;

namespace OllamaSqlClr.Tests.Mocks
{
    public class MockQueryValidator : IQueryValidator
    {
        public bool IsSafeQuery(string query)
        {
            if (query == "mockSafeQuery")
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
    }
}
