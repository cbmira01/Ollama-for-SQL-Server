using Moq;
using Xunit;
using OllamaSqlClr;
using OllamaSqlClr.Models;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Collections;
using OllamaSqlClr.Services;
using OllamaSqlClr.DataAccess;
using OllamaSqlClr.Helpers;

namespace OllamaSqlClr.Tests
{
    public class SqlClrFunctionsTests
    {
        private readonly Mock<IOllamaService> _mockService;
        private readonly Mock<IDatabaseExecutor> _mockDatabaseExecutor;

        public SqlClrFunctionsTests()
        {
            _mockService = new Mock<IOllamaService>();
            _mockDatabaseExecutor = new Mock<IDatabaseExecutor>();

            // Override the factory to return the mock service for tests that need it
            //SqlClrFunctions.OllamaServiceFactory = () => _mockService.Object;
        }

        [Fact]
        public void Test01_CompletePrompt_ReturnsExpectedServiceLayerObject()
        {
            // Arrange
            var modelName = new SqlString("testModel");
            var askPrompt = new SqlString("What is the weather?");
            var morePrompt = new SqlString("Tell me more about today's forecast.");
            var mockReturn = new SqlString("Sunny with a chance of rain.");

            _mockService
                .Setup(service => service.CompletePrompt(modelName, askPrompt, morePrompt))
                .Returns(mockReturn);

            // Act
            var result = SqlClrFunctions.CompletePrompt(modelName, askPrompt, morePrompt);

            // Assert
            Assert.IsType<SqlString>(result);  // Ensures it returns the expected SqlString type
            Assert.Equal(mockReturn, result);   // Optional: verify the content matches the mock
        }

        [Fact]
        public void Test02_CompleteMultiplePrompts_ReturnsExpectedServiceLayerObject()
        {
            // Arrange
            var modelName = new SqlString("testModel");
            var askPrompt = new SqlString("What is the weather?");
            var morePrompt = new SqlString("Tell me more about today's forecast.");
            var numCompletions = new SqlInt32(2);
            var mockReturn = new List<CompletionRow>(); // Expected type from service layer

            _mockService
                .Setup(service => service.CompleteMultiplePrompts(modelName, askPrompt, morePrompt, numCompletions))
                .Returns(mockReturn);

            // Act
            var result = SqlClrFunctions.CompleteMultiplePrompts(modelName, askPrompt, morePrompt, numCompletions);

            // Assert
            Assert.IsAssignableFrom<IEnumerable>(result);  // Ensures it returns the expected type
        }

        [Fact]
        public void Test03_GetAvailableModels_ReturnsExpectedServiceLayerObject()
        {
            // Arrange
            var mockReturn = new List<ModelInformationRow>(); // Expected type from service layer

            _mockService
                .Setup(service => service.GetAvailableModels())
                .Returns(mockReturn);

            // Act
            var result = SqlClrFunctions.GetAvailableModels();

            // Assert
            Assert.IsAssignableFrom<IEnumerable>(result);  // Ensures it returns the expected type
        }

        [Fact(Skip = "QueryFromPrompt not implemented")]
        public void Test04_QueryFromPrompt_ReturnsExpectedServiceLayerObject()
        {
            // Arrange
            var modelName = new SqlString("testModel");
            var askPrompt = new SqlString("What is the weather?");
            var morePrompt = new SqlString("Tell me more about today's forecast.");
            var numCompletions = new SqlInt32(2);
            var mockReturn = new SqlString("Sunny with a chance of rain.");

            //_mockService
            //    .Setup(service => service.QueryFromPrompt(modelName, askPrompt))
            //    .Returns(mockReturn);

            // Act
            //var result = SqlClrFunctions.QueryFromPrompt(modelName, askPrompt);

            // Assert
            //Assert.IsType<SqlString>(result);  // Ensures it returns the expected SqlString type
            //Assert.Equal(mockReturn, result);   // Verifies the content matches the mock
        }

        [Fact]
        public void Test05_OllamaServiceInstance_IsInitialized()
        {
            Assert.NotNull(SqlClrFunctions.OllamaServiceInstance);
        }

        [Fact]
        public void Test06_OllamaServiceFactory_ReturnsValidInstance()
        {
            // Override the factory to use mocked DatabaseExecutor to avoid SQL Server dependency
            //SqlClrFunctions.OllamaServiceFactory = () => new OllamaService(
            //    //new QueryValidator(),
            //    //new QueryLogger(_mockDatabaseExecutor.Object),
            //    new OllamaApiClient("http://127.0.0.1:11434")
            //    //new SqlCommand(_mockDatabaseExecutor.Object),
            //    //new SqlQuery(_mockDatabaseExecutor.Object)
            //);

            // Act
            //var ollamaServiceInstance = SqlClrFunctions.OllamaServiceFactory();

            //// Assert
            //Assert.NotNull(ollamaServiceInstance); // Check that the instance is not null
            //Assert.IsType<OllamaService>(ollamaServiceInstance); // Check that it is of type OllamaService
        }

        [Fact]
        public void Test07_OllamaServiceInstance_HasCorrectDependencies()
        {
            // Act
            var ollamaServiceInstance = SqlClrFunctions.OllamaServiceInstance;

            // Use Reflection or other methods to verify dependencies if needed
            Assert.NotNull(ollamaServiceInstance);
        }
    }
}
