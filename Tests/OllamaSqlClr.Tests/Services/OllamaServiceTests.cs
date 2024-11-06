using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Moq;
using Xunit;
using OllamaSqlClr.Helpers;
using OllamaSqlClr.Models;
using OllamaSqlClr.DataAccess;

namespace OllamaSqlClr.Tests
{
    public class OllamaServiceTests
    {
        private readonly Mock<QueryValidator> _mockValidator;
        private readonly Mock<QueryLogger> _mockLogger;
        private readonly Mock<OllamaApiClient> _mockApiClient;
        private readonly Mock<SqlCommand> _mockSqlCommand;
        private readonly Mock<SqlQuery> _mockSqlQuery;

        private readonly OllamaService _ollamaService;

        public OllamaServiceTests()
        {
            _mockValidator = new Mock<QueryValidator>();
            _mockLogger = new Mock<QueryLogger>(Mock.Of<IDatabaseExecutor>());
            _mockApiClient = new Mock<OllamaApiClient>("http://127.0.0.1:11434");
            _mockSqlCommand = new Mock<SqlCommand>(Mock.Of<IDatabaseExecutor>());
            _mockSqlQuery = new Mock<SqlQuery>(Mock.Of<IDatabaseExecutor>());

            _ollamaService = new OllamaService(
                _mockValidator.Object,
                _mockLogger.Object,
                _mockApiClient.Object,
                _mockSqlCommand.Object,
                _mockSqlQuery.Object);
        }

        [Fact]
        public void CompletePrompt_ReturnsExpectedResponse()
        {
            var modelName = "llama3.2";
            var askPrompt = "Why is the sky blue?";
            var morePrompt = "Answer in less than twenty words.";
            var expectedResponse = new SqlString("The sky is blue due to Rayleigh scattering.");

            _mockApiClient
                .Setup(api => api.GetModelResponseToPrompt(It.IsAny<string>(), modelName))
                .Returns(new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>("response", expectedResponse.Value) });

            var result = _ollamaService.CompletePrompt(modelName, askPrompt, morePrompt);

            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public void CompletePrompt_ReturnsError_OnException()
        {
            var modelName = "llama3.2";
            var askPrompt = "Why is the sky blue?";
            var morePrompt = "Answer in less than twenty words.";

            _mockApiClient
                .Setup(api => api.GetModelResponseToPrompt(It.IsAny<string>(), modelName))
                .Throws(new Exception("API error"));

            var result = _ollamaService.CompletePrompt(modelName, askPrompt, morePrompt);

            Assert.StartsWith("Error:", result.Value);
        }

        [Fact]
        public void CompleteMultiplePrompts_ReturnsMultipleCompletions()
        {
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
                .Setup(api => api.GetModelResponseToPrompt(It.IsAny<string>(), modelName.Value, null))
                .Returns(new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>("response", responseList[0].OllamaCompletion) });

            var result = (IEnumerable<CompletionRow>)_ollamaService.CompleteMultiplePrompts(modelName, askPrompt, morePrompt, numCompletions);

            Assert.Collection(result,
                item => Assert.Equal("Rain is caused by moisture condensing.", item.OllamaCompletion),
                item => Assert.Equal("Rain forms when clouds become saturated.", item.OllamaCompletion));
        }

        [Fact]
        public void GetAvailableModels_ReturnsModelInformation()
        {
            var modelInfo = new ModelInformationRow
            {
                ModelGuid = Guid.NewGuid(),
                Name = "llama3.2",
                Model = "model",
                ReferToName = "llama3",
                ModifiedAt = DateTime.Now,
                Size = 12345,
                Family = "language model",
                ParameterSize = "large",
                QuantizationLevel = "medium",
                Digest = "digest123"
            };

            _mockApiClient
                .Setup(api => api.GetOllamaApiTags())
                .Returns(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("models.length", 1),
                    new KeyValuePair<string, object>("models[0].name", modelInfo.Name),
                    new KeyValuePair<string, object>("models[0].model", modelInfo.Model),
                    new KeyValuePair<string, object>("models[0].modified_at", modelInfo.ModifiedAt),
                    new KeyValuePair<string, object>("models[0].size", modelInfo.Size),
                    new KeyValuePair<string, object>("models[0].details.family", modelInfo.Family),
                    new KeyValuePair<string, object>("models[0].details.parameter_size", modelInfo.ParameterSize),
                    new KeyValuePair<string, object>("models[0].details.quantization_level", modelInfo.QuantizationLevel),
                    new KeyValuePair<string, object>("models[0].digest", modelInfo.Digest)
                });

            var result = (IEnumerable<ModelInformationRow>)_ollamaService.GetAvailableModels();

            Assert.Collection(result,
                item =>
                {
                    Assert.Equal(modelInfo.Name, item.Name);
                    Assert.Equal(modelInfo.Model, item.Model);
                });
        }
    }
}
