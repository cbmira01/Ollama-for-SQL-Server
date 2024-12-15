using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Moq;
using Xunit;
using OllamaSqlClr;
using OllamaSqlClr.Services;
using OllamaSqlClr.Models;

namespace OllamaSqlClr.Tests
{
    public class SqlClrFunctionsTests
    {
        private readonly Mock<IOllamaService> _mockOllamaService;

        public SqlClrFunctionsTests()
        {
            // Mock the service interface
            _mockOllamaService = new Mock<IOllamaService>();

            // Inject the mock service into the static class
            SqlClrFunctions.SetMockOllamaServiceInstance(_mockOllamaService.Object);
        }

        [Fact]
        public void Test01_Configure_ThrowsException_WhenParametersAreNull()
        {
            // Arrange
            string nullSqlConnection = null;
            string nullApiUrl = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => SqlClrFunctions.Configure(nullSqlConnection, "http://api.url"));
            Assert.Throws<ArgumentNullException>(() => SqlClrFunctions.Configure("mockConnection", nullApiUrl));
        }

        [Fact(Skip = "Test disabled because Configure method prevents incomplete configuration.")]
        public void Test02_Constructor_ThrowsException_WhenNotConfigured()
        {
            // Arrange
            SqlClrFunctions.SetMockOllamaServiceInstance(null); // Reset the service instance
            SqlClrFunctions.Configure(null, null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => SqlClrFunctions.OllamaServiceInstance);
        }

        [Fact]
        public void Test03_CompletePrompt_ReturnsExpectedResponse()
        {
            // Arrange
            var modelName = new SqlString("llama3.2");
            var askPrompt = new SqlString("What causes rain?");
            var morePrompt = new SqlString("Explain briefly.");
            var expectedResponse = new SqlString("Rain is caused by moisture in the air condensing.");

            _mockOllamaService
                .Setup(service => service.CompletePrompt(modelName, askPrompt, morePrompt))
                .Returns(expectedResponse);

            // Act
            var result = SqlClrFunctions.CompletePrompt(modelName, askPrompt, morePrompt);

            // Assert
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public void Test04_CompleteMultiplePrompts_ReturnsExpectedCollection()
        {
            // Arrange
            var modelName = new SqlString("llama3.2");
            var askPrompt = new SqlString("What causes rain?");
            var morePrompt = new SqlString("Explain briefly.");
            var numCompletions = new SqlInt32(2);

            var expectedResult = new List<CompletionRow>
            {
                new CompletionRow { CompletionGuid = Guid.NewGuid(), ModelName = modelName.Value, OllamaCompletion = "Rain is caused by moisture condensing." },
                new CompletionRow { CompletionGuid = Guid.NewGuid(), ModelName = modelName.Value, OllamaCompletion = "Rain forms when clouds become saturated." }
            };

            _mockOllamaService
                .Setup(service => service.CompleteMultiplePrompts(modelName, askPrompt, morePrompt, numCompletions))
                .Returns(expectedResult);

            // Act
            var result = SqlClrFunctions.CompleteMultiplePrompts(modelName, askPrompt, morePrompt, numCompletions);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Test05_GetAvailableModels_ReturnsExpectedModels()
        {
            // Arrange
            var expectedModels = new List<ModelInformationRow>
            {
                new ModelInformationRow
                {
                    ModelGuid = Guid.NewGuid(),
                    Name = "Model1",
                    Model = "Type1",
                    ReferToName = "Ref1",
                    ModifiedAt = DateTime.Now,
                    Size = 1024,
                    Family = "Family1",
                    ParameterSize = "Small",
                    QuantizationLevel = "8-bit",
                    Digest = "abc123"
                }
            };

            _mockOllamaService
                .Setup(service => service.GetAvailableModels())
                .Returns(expectedModels);

            // Act
            var result = SqlClrFunctions.GetAvailableModels();

            // Assert
            Assert.Equal(expectedModels, result);
        }

        [Fact]
        public void Test06_QueryFromPrompt_ReturnsExpectedResponse()
        {
            // Arrange
            var modelName = new SqlString("llama3.2");
            var prompt = new SqlString("What emails have a glad sentiment?");

            // Expected values
            var expectedId = Guid.NewGuid();
            var expectedTimestamp = DateTime.UtcNow;
            var expectedProposedQuery = "SELECT * FROM support_emails WHERE sentiment LIKE '%glad%';";
            var expectedJsonResult = "[{\"Email\": \"user1@example.com\", \"Sentiment\": \"glad\"}]";

            var expectedRow = new QueryFromPromptRow
            {
                QueryGuid = expectedId,
                ModelName = modelName.Value,
                Prompt = prompt.Value,
                ProposedQuery = expectedProposedQuery,
                Result = expectedJsonResult,
                Timestamp = expectedTimestamp
            };

            var expectedResult = new List<QueryFromPromptRow> { expectedRow };

            // Mock the service to return the expected result
            _mockOllamaService
                .Setup(service => service.QueryFromPrompt(modelName, prompt))
                .Returns(expectedResult);

            // Act
            var result = SqlClrFunctions.QueryFromPrompt(modelName, prompt);

            // Assert
            var resultEnumerator = result.GetEnumerator();
            var expectedEnumerator = expectedResult.GetEnumerator();

            // Compare each row in the result
            while (resultEnumerator.MoveNext() && expectedEnumerator.MoveNext())
            {
                var actualRow = resultEnumerator.Current as QueryFromPromptRow;
                var expectedRowItem = expectedEnumerator.Current;

                Assert.NotNull(actualRow);
                Assert.Equal(expectedRowItem.QueryGuid, actualRow.QueryGuid);
                Assert.Equal(expectedRowItem.ModelName, actualRow.ModelName);
                Assert.Equal(expectedRowItem.Prompt, actualRow.Prompt);
                Assert.Equal(expectedRowItem.ProposedQuery, actualRow.ProposedQuery);
                Assert.Equal(expectedRowItem.Result, actualRow.Result);
                Assert.Equal(expectedRowItem.Timestamp, actualRow.Timestamp);
            }

            // Ensure both enumerators have been fully consumed
            Assert.False(resultEnumerator.MoveNext());
            Assert.False(expectedEnumerator.MoveNext());
        }
    }
}
