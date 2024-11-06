using System;
using System.Collections.Generic;
using System.Collections;
using System.Data.SqlTypes;
using Moq;
using Xunit;
using OllamaSqlClr;
using OllamaSqlClr.Helpers;
using OllamaSqlClr.Models;
using OllamaSqlClr.DataAccess;

namespace OllamaSqlClr.Tests
{
    public class SqlClrFunctionsTests
    {
        private readonly Mock<OllamaService> _mockOllamaService;

        public SqlClrFunctionsTests()
        {
            _mockOllamaService = new Mock<OllamaService>(
                new QueryValidator(),
                new QueryLogger(new Mock<IDatabaseExecutor>().Object),
                new OllamaApiClient("http://127.0.0.1:11434"),
                new SqlCommand(new Mock<IDatabaseExecutor>().Object),
                new SqlQuery(new Mock<IDatabaseExecutor>().Object)
            );

            // Inject the mock OllamaService instance into the static field of SqlClrFunctions
            typeof(SqlClrFunctions)
                .GetField("_ollamaService", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                .SetValue(null, _mockOllamaService.Object);
        }

        [Fact]
        public void CompletePrompt_ReturnsExpectedResponse()
        {
            var modelName = new SqlString("llama3.2");
            var askPrompt = new SqlString("Why is the sky blue?");
            var morePrompt = new SqlString("Answer in less than twenty words.");
            var expectedResponse = new SqlString("The sky is blue due to Rayleigh scattering.");

            _mockOllamaService
                .Setup(s => s.CompletePrompt(modelName.Value, askPrompt.Value, morePrompt.Value))
                .Returns(expectedResponse);

            var result = SqlClrFunctions.CompletePrompt(modelName, askPrompt, morePrompt);

            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public void CompletePrompt_ReturnsError_OnException()
        {
            var modelName = new SqlString("llama3.2");
            var askPrompt = new SqlString("Why is the sky blue?");
            var morePrompt = new SqlString("Answer in less than twenty words.");

            _mockOllamaService
                .Setup(s => s.CompletePrompt(modelName.Value, askPrompt.Value, morePrompt.Value))
                .Throws(new Exception("API error"));

            var result = SqlClrFunctions.CompletePrompt(modelName, askPrompt, morePrompt);

            Assert.StartsWith("Error:", result.Value);
        }

        [Fact]
        public void CompleteMultiplePrompts_ReturnsMultipleCompletions()
        {
            var modelName = new SqlString("llama3.2");
            var askPrompt = new SqlString("What causes rain?");
            var morePrompt = new SqlString("Explain briefly.");
            var numCompletions = new SqlInt32(2);
            var completionList = new List<CompletionRow>
            {
                new CompletionRow { CompletionGuid = Guid.NewGuid(), ModelName = modelName.Value, OllamaCompletion = "Rain is caused by moisture condensing." },
                new CompletionRow { CompletionGuid = Guid.NewGuid(), ModelName = modelName.Value, OllamaCompletion = "Rain forms when clouds become saturated." }
            };

            _mockOllamaService
                .Setup(s => s.CompleteMultiplePrompts(modelName.Value, askPrompt.Value, morePrompt.Value, numCompletions))
                .Returns(completionList);

            var result = (IEnumerable<CompletionRow>)SqlClrFunctions.CompleteMultiplePrompts(modelName, askPrompt, morePrompt, numCompletions);

            Assert.Collection<CompletionRow>(result,
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

            var modelList = new List<ModelInformationRow> { modelInfo };

            _mockOllamaService
                .Setup(s => s.GetAvailableModels())
                .Returns(modelList);

            var result = (IEnumerable<ModelInformationRow>)SqlClrFunctions.GetAvailableModels();

            Assert.Collection<ModelInformationRow>(result,
                item =>
                {
                    Assert.Equal(modelInfo.Name, item.Name);
                    Assert.Equal(modelInfo.Model, item.Model);
                });
        }

        [Fact]
        public void QueryFromPrompt_ReturnsExpectedSqlResult()
        {
            var modelName = new SqlString("llama3.2");
            var askPrompt = new SqlString("Select users where age > 30");
            var expectedResponse = new SqlString("Query executed successfully.");

            _mockOllamaService
                .Setup(s => s.QueryFromPrompt(modelName.Value, askPrompt.Value))
                .Returns(expectedResponse);

            var result = SqlClrFunctions.QueryFromPrompt(modelName, askPrompt);

            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public void QueryFromPrompt_ReturnsError_WhenQueryIsUnsafe()
        {
            var modelName = new SqlString("llama3.2");
            var askPrompt = new SqlString("Drop table users");

            _mockOllamaService
                .Setup(s => s.QueryFromPrompt(modelName.Value, askPrompt.Value))
                .Returns(new SqlString("Error: proposed query had unsafe keywords."));

            var result = SqlClrFunctions.QueryFromPrompt(modelName, askPrompt);

            Assert.StartsWith("Error:", result.Value);
        }
    }
}
