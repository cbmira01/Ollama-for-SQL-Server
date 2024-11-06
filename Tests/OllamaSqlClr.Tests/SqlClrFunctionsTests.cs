using Moq;
using Xunit;
using OllamaSqlClr;

namespace OllamaSqlClr.Tests
{
    public class SqlClrFunctionsInitializationTests
    {
        private readonly Mock<IOllamaService> _mockService;

        public SqlClrFunctionsInitializationTests()
        {
            _mockService = new Mock<IOllamaService>();

            // Override the factory to return the mock service
            SqlClrFunctions.OllamaServiceFactory = () => _mockService.Object;
        }

        [Fact]
        public void OllamaServiceInstance_IsInitialized()
        {
            Assert.NotNull(SqlClrFunctions.OllamaServiceInstance);
        }

        [Fact]
        public void CompletePrompt_UsesMockedService()
        {
            // Setup and test any method using the mock
        }
    }
}
