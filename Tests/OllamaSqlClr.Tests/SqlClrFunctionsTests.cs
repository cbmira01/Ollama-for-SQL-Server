using Moq;
using Xunit;
using OllamaSqlClr;
using OllamaSqlClr.Models;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Collections;

namespace OllamaSqlClr.Tests
{
    public class SqlClrFunctionsTests
    {
        private readonly Mock<IOllamaService> _mockService;

        public SqlClrFunctionsTests()
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
        public void CompletePrompt_ReturnsExpectedServiceLayerObject()
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
        public void CompleteMultiplePrompts_ReturnsExpectedServiceLayerObject()
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
        public void GetAvailableModels_ReturnsExpectedServiceLayerObject()
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

        [Fact]
        public void QueryFromPrompt_ReturnsExpectedServiceLayerObject()
        {
            // Arrange
            var modelName = new SqlString("testModel");
            var askPrompt = new SqlString("What is the weather?");
            var morePrompt = new SqlString("Tell me more about today's forecast.");
            var numCompletions = new SqlInt32(2);
            var mockReturn = new SqlString("Sunny with a chance of rain.");

            _mockService
                .Setup(service => service.QueryFromPrompt(modelName, askPrompt))
                .Returns(mockReturn);

            // Act
            var result = SqlClrFunctions.QueryFromPrompt(modelName, askPrompt);

            // Assert
            Assert.IsType<SqlString>(result);  // Ensures it returns the expected SqlString type
            Assert.Equal(mockReturn, result);   // Verifies the content matches the mock
        }
    }
}
