using Xunit;
using Moq;
using OllamaSqlClr.Helpers;
using OllamaSqlClr.DataAccess;
using System.Data;

namespace OllamaSqlClr.Tests.Helpers
{
    public class SqlCommandTests
    {
        [Fact]
        public void CreateProcedureFromQuery_ShouldReturnProcedureName()
        {
            // Arrange
            var mockExecutor = new Mock<IDatabaseExecutor>();
            mockExecutor.Setup(e => e.ExecuteNonQuery(It.IsAny<string>())).Verifiable();

            var sqlCommand = new SqlCommand(mockExecutor.Object);
            var query = "SELECT * FROM test";

            // Act
            var result = sqlCommand.CreateProcedureFromQuery(query);

            // Assert
            Assert.StartsWith("#TempProc_", result);
            mockExecutor.Verify(e => e.ExecuteNonQuery(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void DropTemporaryProcedure_ShouldReturnSuccessMessage_WhenDroppingIsSuccessful()
        {
            // Arrange
            var mockExecutor = new Mock<IDatabaseExecutor>();
            mockExecutor.Setup(e => e.ExecuteNonQuery(It.IsAny<string>())).Verifiable();

            var sqlCommand = new SqlCommand(mockExecutor.Object);
            var procedureName = "#TempProc_123";

            // Act
            var (success, message) = sqlCommand.DropTemporaryProcedure(procedureName);

            // Assert
            Assert.True(success);
            Assert.Contains("was dropped successfully", message);
            mockExecutor.Verify(e => e.ExecuteNonQuery(It.IsAny<string>()), Times.Once);
        }
    }
}
