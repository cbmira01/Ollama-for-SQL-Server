using Xunit;
using Moq;
using OllamaSqlClr.Helpers;
using OllamaSqlClr.DataAccess;

namespace OllamaSqlClr.Tests.Helpers
{
    public class QueryLoggerTests
    {
        [Fact]
        public void LogQuerySuccess_ShouldExecuteNonQueryWithCorrectCommand()
        {
            // Arrange
            var mockExecutor = new Mock<IDatabaseExecutor>();
            var logger = new QueryLogger(mockExecutor.Object);

            var prompt = "Sample Prompt";
            var query = "SELECT * FROM TestTable";

            // Act
            logger.LogQuerySuccess(prompt, query);

            // Assert
            mockExecutor.Verify(e => e.ExecuteNonQuery(It.Is<string>(cmd =>
                cmd.Contains("INSERT INTO QueryPromptLog") &&
                cmd.Contains($"'{prompt}'") &&
                cmd.Contains($"'{query}'") &&
                cmd.Contains("''")  // Ensures empty strings instead of NULL for error details
            )), Times.Once);
        }

        [Fact]
        public void LogQueryError_ShouldExecuteNonQueryWithCorrectCommandAndErrorDetails()
        {
            // Arrange
            var mockExecutor = new Mock<IDatabaseExecutor>();
            var logger = new QueryLogger(mockExecutor.Object);

            var prompt = "Sample Prompt";
            var query = "SELECT * FROM TestTable";
            var errorNumber = "1234";
            var errorMessage = "Sample error message";
            var errorLine = "45";

            // Act
            logger.LogQueryError(prompt, query, errorNumber, errorMessage, errorLine);

            // Assert
            mockExecutor.Verify(e => e.ExecuteNonQuery(It.Is<string>(cmd =>
                cmd.Contains("INSERT INTO QueryPromptLog") &&
                cmd.Contains($"'{prompt}'") &&
                cmd.Contains($"'{query}'") &&
                cmd.Contains($"'{errorNumber}'") &&
                cmd.Contains($"'{errorMessage}'") &&
                cmd.Contains($"'{errorLine}'")
            )), Times.Once);
        }
    }
}
