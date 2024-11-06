using Xunit;
using Moq;
using OllamaSqlClr.Helpers;
using OllamaSqlClr.DataAccess;
using System.Data;
using System.Data.SqlClient;

namespace OllamaSqlClr.Tests.Helpers
{
    public class SqlQueryTests
    {
        [Fact]
        public void ExecuteProcedure_ShouldReturnDataTable_WhenProcedureExecutesSuccessfully()
        {
            // Arrange
            var mockExecutor = new Mock<IDatabaseExecutor>();
            mockExecutor.Setup(e => e.GetConnection()).Returns(new SqlConnection("mock connection string"));

            var sqlQuery = new SqlQuery(mockExecutor.Object);
            var procedureName = "mockProcedure";

            // Act
            var result = sqlQuery.ExecuteProcedure(procedureName);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<DataTable>(result);
        }
    }
}
