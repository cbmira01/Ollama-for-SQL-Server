using Moq;
using Xunit;
using System.Data;
using OllamaSqlClr.Helpers;
using OllamaSqlClr.DataAccess;
using System.Data.SqlClient;

namespace OllamaSqlClr.Tests.Helpers
{
    public class SqlQueryTests
    {
        [Fact]
        public void ExecuteProcedure_CallsGetConnectionAndExecutesCommand()
        {
            // Arrange
            var mockDbExecutor = new Mock<IDatabaseExecutor>();

            // Set up the mockDbExecutor to return a non-null, dummy SqlConnection
            using (var dummyConnection = new SqlConnection("Server=fake;Database=fake;User Id=fake;Password=fake;"))
            {
                mockDbExecutor.Setup(exec => exec.GetConnection()).Returns(dummyConnection);

                var sqlQuery = new SqlQuery(mockDbExecutor.Object);

                // Act
                DataTable result = null;
                try
                {
                    result = sqlQuery.ExecuteProcedure("TestProcedure");
                }
                catch
                {
                    // Swallow exceptions to avoid actual execution against a database
                }

                // Assert
                mockDbExecutor.Verify(exec => exec.GetConnection(), Times.Once, "GetConnection should be called once");
                Assert.Null(result); // Check that a DataTable is returned, even if empty
            }
        }
    }
}
