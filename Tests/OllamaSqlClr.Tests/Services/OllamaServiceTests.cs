using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using Moq;
using Xunit;

using OllamaSqlClr.Services;
using OllamaSqlClr.Helpers;
using OllamaSqlClr.Models;
using JsonClrLibrary;

namespace OllamaSqlClr.Tests.Services
{
    public class OllamaServiceTests
    {
        private readonly Mock<IQueryValidator> _mockValidator;
        private readonly Mock<IQueryLogger> _mockLogger;
        private readonly Mock<IOllamaApiClient> _mockApiClient;
        private readonly Mock<ISqlCommand> _mockSqlCommand;
        private readonly Mock<ISqlQuery> _mockSqlQuery;
        private readonly OllamaService _ollamaService;

        public OllamaServiceTests()
        {
            _mockValidator = new Mock<IQueryValidator>();
            _mockLogger = new Mock<IQueryLogger>();
            _mockApiClient = new Mock<IOllamaApiClient>();
            _mockSqlCommand = new Mock<ISqlCommand>();
            _mockSqlQuery = new Mock<ISqlQuery>();

            //_ollamaService = new OllamaService(
            //    //_mockValidator.Object,
            //    //_mockLogger.Object,
            //    _mockApiClient.Object
            //    //_mockSqlCommand.Object,
            //    //_mockSqlQuery.Object
            //);
        }

        [Fact]
        public void Test01_CompletePrompt_ReturnsExpectedResponse()
        {
            var modelName = "llama3.2";
            var askPrompt = "What causes rain?";
            var morePrompt = "Explain briefly.";
            var expectedResponse = "Rain is caused by moisture in the air condensing.";

            _mockApiClient
                .Setup(api => api.GetModelResponseToPrompt(It.IsAny<string>(), modelName))
                .Returns(new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>("response", expectedResponse) });

            var result = _ollamaService.CompletePrompt(modelName, askPrompt, morePrompt);

            Assert.Equal(expectedResponse, result.Value);
        }

        [Fact]
        public void Test02_CompleteMultiplePrompts_ReturnsMultipleCompletions_WithContextUpdate()
        {
            var modelName = new SqlString("llama3.2");
            var askPrompt = new SqlString("What causes rain?");
            var morePrompt = new SqlString("Explain briefly.");
            var numCompletions = new SqlInt32(2);

            // Mock responses with different contexts for each iteration
            var responseList = new List<CompletionRow>
            {
                new CompletionRow { CompletionGuid = Guid.NewGuid(), ModelName = modelName.Value, OllamaCompletion = "Rain is caused by moisture condensing." },
                new CompletionRow { CompletionGuid = Guid.NewGuid(), ModelName = modelName.Value, OllamaCompletion = "Rain forms when clouds become saturated." }
            };

            // Define different contexts to simulate the API's response
            var contextArray1 = new List<object> { 1, 2, 3 };
            var contextArray2 = new List<object> { 4, 5, 6 };

            _mockApiClient
                .SetupSequence(api => api.GetModelResponseToPrompt(It.IsAny<string>(), modelName.Value, It.IsAny<List<int>>()))
                .Returns(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("response", responseList[0].OllamaCompletion),
                    new KeyValuePair<string, object>("context", contextArray1)
                })
                .Returns(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("response", responseList[1].OllamaCompletion),
                    new KeyValuePair<string, object>("context", contextArray2)
                });

            // Act
            var result = (IEnumerable<CompletionRow>)_ollamaService.CompleteMultiplePrompts(modelName, askPrompt, morePrompt, numCompletions);

            // Assert: Ensure responses match the expected completions
            Assert.Collection(result,
                item => Assert.Equal("Rain is caused by moisture condensing.", item.OllamaCompletion),
                item => Assert.Equal("Rain forms when clouds become saturated.", item.OllamaCompletion));
        }

        [Fact]
        public void Test03_GetAvailableModels_ReturnsModelInformation()
        {
            // Arrange
            var modelData = JsonBuilder.CreateAnonymousObject(
                JsonBuilder.CreateField("name", "Model1"),
                JsonBuilder.CreateField("model", "ModelType1"),
                JsonBuilder.CreateField("modified_at", DateTime.Now.ToString("o")), // Date as ISO 8601 string
                JsonBuilder.CreateNumeric("size", 1024L),
                JsonBuilder.CreateObject("details",
                    JsonBuilder.CreateField("family", "TestFamily"),
                    JsonBuilder.CreateField("parameter_size", "1024MB"),
                    JsonBuilder.CreateField("quantization_level", "8-bit")
                ),
                JsonBuilder.CreateField("digest", "abc123")
            );

            // Mock API response to return a structure that includes `models.length`
            _mockApiClient
                .Setup(api => api.GetOllamaApiTags())
                .Returns(new List<KeyValuePair<string, object>>
                {
                    JsonBuilder.CreateArray("models", modelData),
                    JsonBuilder.CreateNumeric("models.length", 1)
                });

            // Act
            var result = (List<ModelInformationRow>)_ollamaService.GetAvailableModels();

            // Assert
            Assert.Single(result);
            Assert.Equal("Model1", result[0].Name);
            Assert.Equal("ModelType1", result[0].Model);
            Assert.Equal("TestFamily", result[0].Family);
            Assert.Equal("1024MB", result[0].ParameterSize);
            Assert.Equal("8-bit", result[0].QuantizationLevel);
            Assert.Equal("abc123", result[0].Digest);
            Assert.Equal(1024L, result[0].Size);
        }

        [Fact(Skip = "QueryFromPrompt not implemented")]
        public void Test04_QueryFromPrompt_ReturnsError_WhenQueryIsNotSafe()
        {
            var modelName = new SqlString("llama3.2");
            var askPrompt = new SqlString("How to delete data?");
            var unsafeQuery = "DROP TABLE Users;";

            _mockValidator.Setup(v => v.IsSafeQuery(unsafeQuery)).Returns(false);

            //var result = _ollamaService.QueryFromPrompt(modelName, askPrompt);

            //Assert.Equal("Error: proposed query had unsafe keywords.", result.Value);
        }

        [Fact(Skip = "QueryFromPrompt not implemented")]
        public void Test05_QueryFromPrompt_ReturnsExecutionResult()
        {
            var modelName = new SqlString("llama3.2");
            var askPrompt = new SqlString("Get data from support_emails.");
            const string safeQuery = "SELECT * FROM support_emails WHERE sentiment = 'glad';"; // Matching the hardcoded query

            var dataTable = new DataTable();
            dataTable.Columns.Add("id", typeof(int));
            dataTable.Rows.Add(1);  // Ensure there’s at least one row

            // Setup mocks
            _mockValidator.Setup(v => v.IsSafeQuery(safeQuery)).Returns(true);
            _mockSqlCommand.Setup(s => s.CreateProcedureFromQuery(safeQuery)).Returns("procedureName");
            _mockSqlCommand.Setup(s => s.RunTemporaryProcedure("procedureName")).Returns(dataTable);
            _mockSqlCommand.Setup(s => s.DropTemporaryProcedure("procedureName")).Returns((true, "Procedure dropped successfully."));

            // Act
            //var result = _ollamaService.QueryFromPrompt(modelName, askPrompt);

            // Assert
            //Assert.Equal("Query executed successfully.", result.Value);
            //_mockValidator.Verify(v => v.IsSafeQuery(It.IsAny<string>()), Times.Once, "IsSafeQuery should be called once but wasn't.");
        }
    }
}
