using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using Moq;
using Xunit;

using OllamaSqlClr.Services;
using OllamaSqlClr.Helpers;
using OllamaSqlClr.DataAccess;
using OllamaSqlClr.Models;
using JsonClrLibrary;
using System.Linq;

namespace OllamaSqlClr.Tests.Services
{
    public class OllamaServiceTests
    {
        private readonly Mock<IDatabaseExecutor> _mockDatabaseExecutor;
        private readonly Mock<IQueryValidator> _mockQueryValidator;
        private readonly Mock<IQueryLogger> _mockQueryLogger;
        private readonly Mock<IOllamaApiClient> _mockApiClient;
        private readonly Mock<ISqlCommandHelper> _mockSqlCommandHelper;
        private readonly Mock<ISqlQueryHelper> _mockSqlQueryHelper;
        private readonly OllamaService _ollamaService;

        public OllamaServiceTests()
        {
            // Mock dependencies
            _mockDatabaseExecutor = new Mock<IDatabaseExecutor>();
            _mockQueryLogger = new Mock<IQueryLogger>();
            _mockApiClient = new Mock<IOllamaApiClient>();
            _mockSqlCommandHelper = new Mock<ISqlCommandHelper>();
            _mockSqlQueryHelper = new Mock<ISqlQueryHelper>();
            _mockQueryValidator = new Mock<IQueryValidator>();

            // Inject mocks into the service
            var apiUrl = "http://127.0.0.1:11434";
            _ollamaService = new OllamaService(
                sqlConnection: "mockConnection",
                apiUrl: apiUrl,
                queryLogger: _mockQueryLogger.Object,
                sqlCommandHelper: _mockSqlCommandHelper.Object,
                sqlQueryHelper: _mockSqlQueryHelper.Object,
                queryValidator: _mockQueryValidator.Object,
                apiClient: _mockApiClient.Object,
                databaseExecutor: _mockDatabaseExecutor.Object);
        }

        [Fact]
        public void Test01_CompletePrompt_ReturnsExpectedResponse()
        {
            // Arrange
            var modelName = "llama3.2";
            var askPrompt = "What causes rain?";
            var morePrompt = "Explain briefly.";
            var expectedResponse = "Rain is caused by moisture in the air condensing.";

            _mockApiClient
                .Setup(api => api.GetModelResponseToPrompt(It.IsAny<string>(), modelName))
                .Returns(new List<KeyValuePair<string, object>> {
                    new KeyValuePair<string, object>("response", expectedResponse)
                });

            // Act
            var result = _ollamaService.CompletePrompt(modelName, askPrompt, morePrompt);

            // Assert
            Assert.Equal(expectedResponse, result.Value);
            _mockApiClient.Verify(api => api.GetModelResponseToPrompt(It.IsAny<string>(), modelName), Times.Once);
        }

        [Fact]
        public void Test02_CompleteMultiplePrompts_ReturnsMultipleCompletions_WithContextUpdate()
        {
            // Arrange
            var modelName = new SqlString("llama3.2");
            var askPrompt = new SqlString("What causes rain?");
            var morePrompt = new SqlString("Explain briefly.");
            var numCompletions = new SqlInt32(2);

            var responseList = new List<CompletionRow>
            {
                new CompletionRow { CompletionGuid = Guid.NewGuid(), ModelName = modelName.Value, OllamaCompletion = "Rain is caused by moisture condensing." },
                new CompletionRow { CompletionGuid = Guid.NewGuid(), ModelName = modelName.Value, OllamaCompletion = "Rain forms when clouds become saturated." }
            };

            _mockApiClient
                .SetupSequence(api => api.GetModelResponseToPrompt(It.IsAny<string>(), modelName.Value, It.IsAny<List<int>>()))
                .Returns(new List<KeyValuePair<string, object>> {
                    new KeyValuePair<string, object>("response", "Rain is caused by moisture condensing."),
                    new KeyValuePair<string, object>("context", new List<int> { 1, 2, 3 }) // Mock context
                })
                .Returns(new List<KeyValuePair<string, object>> {
                    new KeyValuePair<string, object>("response", "Rain forms when clouds become saturated."),
                    new KeyValuePair<string, object>("context", new List<int> { 4, 5, 6 }) // Mock context
                });

            // Act
            var result = _ollamaService.CompleteMultiplePrompts(modelName, askPrompt, morePrompt, numCompletions);
            var resultList = result.Cast<CompletionRow>().ToList();

            // Assert
            Assert.Collection(resultList,
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

            _mockApiClient
                .Setup(api => api.GetOllamaApiTags())
                .Returns(new List<KeyValuePair<string, object>> {
                    JsonBuilder.CreateArray("models", modelData),
                    JsonBuilder.CreateNumeric("models.length", 1)
                });

            // Act
            var result = _ollamaService.GetAvailableModels();
            var resultList = result.Cast<ModelInformationRow>().ToList();

            // Assert
            Assert.Single(resultList);
            Assert.Equal("Model1", resultList[0].Name);
            Assert.Equal("ModelType1", resultList[0].Model);
        }
    }
}
