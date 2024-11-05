using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;

using OllamaSqlClr.DataAccess;
using JsonClrLibrary;
using static OllamaSqlClr.OllamaHelpers;

namespace OllamaSqlClr.Tests
{
    public class SqlClrFunctionsTests
    {
        private readonly Mock<IOllamaHelper> _mockOllamaHelper;
        private readonly Mock<IJsonSerializerDeserializer> _mockJsonHelper;
        private readonly Mock<IDatabaseExecutor> _mockDbExecutor;

        public SqlClrFunctionsTests()
        {
            _mockOllamaHelper = new Mock<IOllamaHelper>();
            _mockJsonHelper = new Mock<IJsonSerializerDeserializer>();
            _mockDbExecutor = new Mock<IDatabaseExecutor>();
        }

        #region CompletePrompt Tests

        [Fact]
        public void CompletePrompt_ReturnsExpectedResponse()
        {
            // Arrange
            var modelName = new SqlString("testModel");
            var askPrompt = new SqlString("What is the capital of France?");
            var morePrompt = new SqlString("Please answer.");
            var expectedResponse = "Paris";

            _mockOllamaHelper.Setup(h => h.GetModelResponseToPrompt(It.IsAny<string>(), modelName.Value))
                .Returns($"{{ \"response\": \"{expectedResponse}\" }}");

            _mockJsonHelper.Setup(j => j.GetStringField(It.IsAny<string>(), "response")).Returns(expectedResponse);

            // Act
            var result = SqlClrFunctions.CompletePrompt(modelName, askPrompt, morePrompt);

            // Assert
            Assert.Equal(expectedResponse, result.Value);
        }

        [Fact]
        public void CompletePrompt_ReturnsErrorOnException()
        {
            // Arrange
            var modelName = new SqlString("testModel");
            var askPrompt = new SqlString("What is the capital of France?");
            var morePrompt = new SqlString("Please answer.");
            var exceptionMessage = "Test exception";

            _mockOllamaHelper.Setup(h => h.GetModelResponseToPrompt(It.IsAny<string>(), modelName.Value))
                .Throws(new Exception(exceptionMessage));

            // Act
            var result = SqlClrFunctions.CompletePrompt(modelName, askPrompt, morePrompt);

            // Assert
            Assert.StartsWith("Error:", result.Value);
        }

        #endregion

        #region CompleteMultiplePrompts Tests

        [Fact]
        public void CompleteMultiplePrompts_ReturnsExpectedCompletions()
        {
            // Arrange
            var modelName = new SqlString("testModel");
            var askPrompt = new SqlString("What is 2+2?");
            var morePrompt = new SqlString("Answer briefly.");
            var numCompletions = new SqlInt32(2);
            var expectedResponses = new List<string> { "4", "4" };
            var contextData = new List<int> { 1, 2, 3 };

            _mockOllamaHelper.SetupSequence(h => h.GetModelResponseToPrompt(It.IsAny<string>(), modelName.Value, It.IsAny<List<int>>()))
                .Returns($"{{ \"response\": \"{expectedResponses[0]}\", \"context\": [1, 2, 3] }}")
                .Returns($"{{ \"response\": \"{expectedResponses[1]}\", \"context\": [1, 2, 3] }}");

            _mockJsonHelper.Setup(j => j.GetStringField(It.IsAny<string>(), "response")).Returns(expectedResponses[0]);
            _mockJsonHelper.Setup(j => j.GetIntegerArray(It.IsAny<string>(), "context")).Returns(contextData);

            // Act
            var result = SqlClrFunctions.CompleteMultiplePrompts(modelName, askPrompt, morePrompt, numCompletions);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(numCompletions.Value, ((List<SqlClrFunctions.CompletionInfo>)result).Count);
        }

        [Fact]
        public void CompleteMultiplePrompts_ReturnsErrorOnException()
        {
            // Arrange
            var modelName = new SqlString("testModel");
            var askPrompt = new SqlString("What is 2+2?");
            var morePrompt = new SqlString("Answer briefly.");
            var numCompletions = new SqlInt32(2);
            var exceptionMessage = "Test exception";

            _mockOllamaHelper.Setup(h => h.GetModelResponseToPrompt(It.IsAny<string>(), modelName.Value, null))
                .Throws(new Exception(exceptionMessage));

            // Act
            var result = SqlClrFunctions.CompleteMultiplePrompts(modelName, askPrompt, morePrompt, numCompletions);

            // Assert
            var completionInfo = Assert.Single((IEnumerable<SqlClrFunctions.CompletionInfo>)result);
            Assert.Equal(Guid.Empty, completionInfo.CompletionGuid);
            Assert.Contains("Error:", completionInfo.OllamaCompletion);
        }

        #endregion

        #region GetAvailableModels Tests

        [Fact]
        public void GetAvailableModels_ReturnsExpectedModels()
        {
            // Arrange
            var modelJson = @"
            {
                'models': [
                    { 'name': 'modelA', 'model': 'A', 'modified_at': '2024-01-01', 'size': 123456, 'details': { 'family': 'TestFamily', 'parameter_size': 'small', 'quantization_level': 'medium' }, 'digest': 'abc123' },
                    { 'name': 'modelB', 'model': 'B', 'modified_at': '2024-01-01', 'size': 654321, 'details': { 'family': 'TestFamily', 'parameter_size': 'large', 'quantization_level': 'high' }, 'digest': 'xyz789' }
                ]
            }";

            _mockOllamaHelper.Setup(h => h.GetOllamaApiTags()).Returns(modelJson);
            _mockJsonHelper.Setup(j => j.GetIntegerByPath(It.IsAny<string>(), "models.length")).Returns(2);
            _mockJsonHelper.Setup(j => j.GetStringByPath(It.IsAny<string>(), "models[0].name")).Returns("modelA");

            // Act
            var result = SqlClrFunctions.GetAvailableModels();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, ((List<SqlClrFunctions.ModelInfo>)result).Count);
        }

        [Fact]
        public void GetAvailableModels_ReturnsErrorOnException()
        {
            // Arrange
            var exceptionMessage = "Test exception";

            _mockOllamaHelper.Setup(h => h.GetOllamaApiTags()).Throws(new Exception(exceptionMessage));

            // Act
            var result = SqlClrFunctions.GetAvailableModels();

            // Assert
            var modelInfo = Assert.Single((IEnumerable<SqlClrFunctions.ModelInfo>)result);
            Assert.Equal(Guid.Empty, modelInfo.ModelGuid);
            Assert.Contains("Error:", modelInfo.Name);
        }

        #endregion

        #region QueryFromPrompt Tests

        [Fact]
        public void QueryFromPrompt_ReturnsSuccessMessage_WhenQueryIsSafe()
        {
            // Arrange
            var modelName = new SqlString("testModel");
            var askPrompt = new SqlString("Fetch all records.");
            var safeQuery = "SELECT * FROM MyTable";
            var dataTable = new DataTable();
            dataTable.Columns.Add("SomeColumn");

            _mockOllamaHelper.Setup(h => h.IsSafe(It.IsAny<string>())).Returns(true);
            _mockDbExecutor.Setup(db => db.BuildAndRunTempProcedure(It.IsAny<string>(), _mockDbExecutor.Object)).Returns(dataTable);

            // Act
            var result = SqlClrFunctions.QueryFromPrompt(modelName, askPrompt, _mockDbExecutor.Object);

            // Assert
            Assert.Equal("Query executed successfully.", result.Value);
        }

        [Fact]
        public void QueryFromPrompt_ReturnsErrorForUnsafeQuery()
        {
            // Arrange
            var modelName = new SqlString("testModel");
            var askPrompt = new SqlString("Fetch all records with unsafe keywords.");

            _mockOllamaHelper.Setup(h => h.IsSafe(It.IsAny<string>())).Returns(false);

            // Act
            var result = SqlClrFunctions.QueryFromPrompt(modelName, askPrompt, _mockDbExecutor.Object);

            // Assert
            Assert.Equal("Error: proposed query had unsafe keywords.", result.Value);
        }

        [Fact]
        public void QueryFromPrompt_ReturnsErrorOnException()
        {
            // Arrange
            var modelName = new SqlString("testModel");
            var askPrompt = new SqlString("Fetch all records.");
            var exceptionMessage = "Test exception";

            _mockDbExecutor.Setup(db => db.BuildAndRunTempProcedure(It.IsAny<string>(), _mockDbExecutor.Object)).Throws(new Exception(exceptionMessage));

            // Act
            var result = SqlClrFunctions.QueryFromPrompt(modelName, askPrompt, _mockDbExecutor.Object);

            // Assert
            Assert.StartsWith("Exception error:", result.Value);
        }

        #endregion
    }
}

