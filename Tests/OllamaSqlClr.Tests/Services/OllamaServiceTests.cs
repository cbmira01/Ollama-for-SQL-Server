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
using Moq.Protected;

namespace OllamaSqlClr.Tests.Services
{
    public class OllamaServiceTests
    {
        private readonly Mock<IDatabaseExecutor> _mockDatabaseExecutor;
        private readonly Mock<IQueryValidator> _mockQueryValidator;
        private readonly Mock<IQueryLogger> _mockQueryLogger;
        private readonly Mock<IOllamaApiClient> _mockApiClient;
        private readonly OllamaService _ollamaService;

        public OllamaServiceTests()
        {
            // Mock dependencies
            _mockDatabaseExecutor = new Mock<IDatabaseExecutor>();
            _mockQueryLogger = new Mock<IQueryLogger>();
            _mockApiClient = new Mock<IOllamaApiClient>();
            _mockQueryValidator = new Mock<IQueryValidator>();

            // Inject mocks into the service
            var apiUrl = "http://127.0.0.1:11434";
            _ollamaService = new OllamaService(
                sqlConnection: "mockConnection",
                apiUrl: apiUrl,
                queryLogger: _mockQueryLogger.Object,
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
            Assert.Equal(expectedResponse, result);
            _mockApiClient.Verify(api => api.GetModelResponseToPrompt(It.IsAny<string>(), modelName), Times.Once);
        }

        [Fact]
        public void Test02_CompleteMultiplePrompts_ReturnsMultipleCompletions_WithContextUpdate()
        {
            // Arrange
            var modelName = "llama3.2";
            var askPrompt = "What causes rain?";
            var morePrompt = "Explain briefly.";
            var numCompletions = 2;

            var responseList = new List<CompletionRow>
            {
                new CompletionRow { CompletionGuid = Guid.NewGuid(), ModelName = modelName, OllamaCompletion = "Rain is caused by moisture condensing." },
                new CompletionRow { CompletionGuid = Guid.NewGuid(), ModelName = modelName, OllamaCompletion = "Rain forms when clouds become saturated." }
            };

            _mockApiClient
                .SetupSequence(api => api.GetModelResponseToPrompt(It.IsAny<string>(), modelName, It.IsAny<List<int>>()))
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

        [Fact(Skip = "Still trying to figure out how to unit test this method.")]
        public void Test04_QueryFromPrompt_ReturnsProposedQueryAndResults()
        {
            // Arrange
            var modelName = "mistral";
            var prompt = "What was the date and time of the earliest purchase?";
            var proposedQuery = "SELECT MIN(SaleDate) AS [Earliest Purchase Date] FROM Sales; -- attempt 1";

            var mockDatabaseExecutor = new Mock<IDatabaseExecutor>();
            var mockApiClient = new Mock<IOllamaApiClient>();

            var dataTable = new DataTable();
            dataTable.Columns.Add("Earliest Purchase Date");
            dataTable.Rows.Add("12/1/2024 10:15:00 AM"); // Mocked data

            // Mock database executor to return a DataTable
            mockDatabaseExecutor.Setup(db => db.ExecuteQuery(proposedQuery)).Returns(dataTable);

            // Mock API client to return a mocked response
            mockApiClient.Setup(api => api.GetModelResponseToPrompt(It.IsAny<string>(), modelName, It.IsAny<List<int>>()))
                .Returns(new List<KeyValuePair<string, object>> {
                    new KeyValuePair<string, object>("response", proposedQuery),
                    new KeyValuePair<string, object>("context", new List<int> { 1, 2, 3 })
                });

            // Create a partial mock of the service
            var serviceMock = new Mock<OllamaService>(mockApiClient.Object, mockDatabaseExecutor.Object) { CallBase = true };

            // Mock the private methods
            serviceMock
                .Protected()
                .Setup<string>("CleanQuery", ItExpr.IsAny<string>())
                .Returns(proposedQuery);

            serviceMock
                .Protected()
                .Setup<string>("ConvertDataTableToJson", ItExpr.IsAny<DataTable>())
                .Returns("[{\"Earliest Purchase Date\": \"12/1/2024 10:15:00 AM\"}]");

            // Act
            var result = serviceMock.Object.QueryFromPrompt(modelName, prompt);
            var resultList = result.Cast<QueryFromPromptRow>().ToList();

            // Assert
            Assert.Collection(resultList,
                item =>
                {
                    Assert.Equal(proposedQuery, item.ProposedQuery);
                    Assert.Equal("[{\"Earliest Purchase Date\": \"12/1/2024 10:15:00 AM\"}]", item.Result);
                });
        }

        [Fact(Skip = "This unit test is in progress.")]
        public void Test05_ExamineImage_ReturnsModelResponse()
        { }
    }
}
