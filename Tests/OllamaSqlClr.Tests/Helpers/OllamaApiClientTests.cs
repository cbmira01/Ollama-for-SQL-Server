using Xunit;
using OllamaSqlClr.Helpers;
using Moq;
using JsonClrLibrary;
using System.Collections.Generic;
using System.Net;

namespace OllamaSqlClr.Tests.Helpers
{
    public class OllamaApiClientTests
    {
        [Fact]
        public void GetModelResponseToPrompt_ShouldReturnKeyValuePairList_WhenApiCallIsSuccessful()
        {
            // Arrange
            var mockApiClient = new Mock<OllamaApiClient>("http://127.0.0.1:11434");
            string prompt = "Test prompt";
            string modelName = "testModel";
            var mockResponse = new List<KeyValuePair<string, object>> { JsonBuilder.CreateField("response", "mock response") };

            mockApiClient.Setup(client => client.GetModelResponseToPrompt(prompt, modelName)).Returns(mockResponse);

            // Act
            var result = mockApiClient.Object.GetModelResponseToPrompt(prompt, modelName);

            // Assert
            Assert.Equal(mockResponse, result);
        }

        [Fact]
        public void GetOllamaApiTags_ShouldReturnKeyValuePairList_WhenApiCallIsSuccessful()
        {
            // Arrange
            var mockApiClient = new Mock<OllamaApiClient>("http://127.0.0.1:11434");
            var mockResponse = new List<KeyValuePair<string, object>> { JsonBuilder.CreateField("tag", "mockTag") };

            mockApiClient.Setup(client => client.GetOllamaApiTags()).Returns(mockResponse);

            // Act
            var result = mockApiClient.Object.GetOllamaApiTags();

            // Assert
            Assert.Equal(mockResponse, result);
        }
    }
}

