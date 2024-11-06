using Xunit;
using Moq;
using OllamaSqlClr.Helpers;
using OllamaSqlClr.DataAccess;

namespace OllamaSqlClr.Tests.Helpers
{
    public class QueryLoggerTests
    {
        [Fact]
        public void LogQuerySuccess_ShouldLogQueryWithoutErrors()
        {
            // Arrange
            var mockExecutor = new Mock<IDatabaseExecutor>();
            mockExecutor.Setup(e => e.ExecuteNonQuery(It.IsAny<string>())).Verifiable();

            var logger = new QueryLogger(mockExecutor.Object);
            var prompt = "Test prompt";
            var query = "SELECT * FROM test";

            // Act
            logger.LogQuerySuccess(prompt, query);

            // Assert
            mockExecutor.Verify(e => e.ExecuteNonQuery(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void LogQueryError_ShouldLogQueryWithErrorDetails()
        {
            // Arrange
            var mockExecutor = new Mock<IDatabaseExecutor>();
            mockExecutor.Setup(e => e.ExecuteNonQuery(It.IsAny<string>())).Verifiable();

            var logger = new QueryLogger(mockExecutor.Object);
            var prompt = "Test prompt";
            var query = "SELECT * FROM test";
            var errorNumber = "123";
            var errorMessage = "Test error";
            var errorLine = "45";

            // Act
            logger.LogQueryError(prompt, query, errorNumber, errorMessage, errorLine);

            // Assert
            mockExecutor.Verify(e => e.ExecuteNonQuery(It.IsAny<string>()), Times.Once);
        }
    }
}
