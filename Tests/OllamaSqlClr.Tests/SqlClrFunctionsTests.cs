using System.Data.SqlTypes;
using Moq;
using OllamaSqlClr.DataAccess;
using OllamaSqlClr.Helpers;
using Xunit;

namespace OllamaSqlClr.Tests
{
    public class SqlClrFunctionsTests
    {
        [Fact]
        public void CompletePrompt_ReturnsExpectedCompletion()
        {
            // Arrange
            var mockOllamaService = new Mock<OllamaService>(
                MockBehavior.Strict,
                new QueryValidator(),
                new QueryLogger(new Mock<IDatabaseExecutor>().Object),
                new OllamaApiClient("http://127.0.0.1:11434"),
                new SqlCommand(new Mock<IDatabaseExecutor>().Object),
                new SqlQuery(new Mock<IDatabaseExecutor>().Object)
            );

            // Mock the expected behavior of CompletePrompt in OllamaService
            mockOllamaService.Setup(service => service.CompletePrompt(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(new SqlString("The sky is blue due to scattering of sunlight."));

            // Substitute OllamaService with the mock in SqlClrFunctions
            var modelName = new SqlString("llama3.2");
            var askPrompt = new SqlString("Why is the sky blue?");
            var morePrompt = new SqlString("Answer in less than twenty words.");

            // Act
            var result = SqlClrFunctions.CompletePrompt(modelName, askPrompt, morePrompt);

            // Assert
            Assert.Equal("The sky is blue due to scattering of sunlight.", result.Value);
        }
    }
}

