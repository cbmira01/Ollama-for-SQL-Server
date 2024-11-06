using Moq;
using Xunit;
using System.Data;
using System.Data.SqlTypes;

using OllamaSqlClr.DataAccess;
using static OllamaSqlClr.SqlClrFunctions;

namespace OllamaSqlClr.Tests.DataAccess
{
    public class QueryFromPromptTests
    {
        [Fact]
        public void QueryFromPrompt_ValidQuery_ReturnsSuccess()
        {
            // Arrange: Create a mock for IDatabaseExecutor
            var mockDbExecutor = new Mock<IDatabaseExecutor>();

            // Set up the mock to return an empty DataTable (simulating a successful query)
            mockDbExecutor
                .Setup(executor => executor.ExecuteQuery(It.IsAny<string>()))
                .Returns(new DataTable());

            // Optional: Verify that ExecuteNonQuery is called for logging purposes
            mockDbExecutor
                .Setup(executor => executor.ExecuteNonQuery(It.IsAny<string>()))
                .Verifiable();

            var modelName = new SqlString("llama3.2");
            var askPrompt = new SqlString("Some valid SQL prompt");

            // Act: Call the method with the mock object
            var result = QueryFromPrompt(modelName, askPrompt);

            // Assert: Verify the results and mock interactions
            Assert.Equal("Query . successfully.", result.Value);
            mockDbExecutor.Verify(executor => executor.ExecuteNonQuery(It.IsAny<string>()), Times.Once);
        }
    }
}
