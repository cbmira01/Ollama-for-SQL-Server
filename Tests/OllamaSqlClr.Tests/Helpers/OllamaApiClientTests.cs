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
        private readonly Mock<IOllamaApiClient> _mockApiClient;

        public OllamaApiClientTests()
        {
            _mockApiClient = new Mock<IOllamaApiClient>();
        }

        [Fact]
        public void GetModelResponseToPrompt_ShouldReturnKeyValuePairList_WhenApiCallIsSuccessful()
        {
            // Arrange
            string prompt = "Test prompt";
            string modelName = "testModel";
            var mockResponse = JsonBuilder.CreateAnonymousObject(JsonBuilder.CreateField("response", "mock response"));

            _mockApiClient.Setup(client => client.GetModelResponseToPrompt(prompt, modelName)).Returns(mockResponse);

            // Act
            var result = _mockApiClient.Object.GetModelResponseToPrompt(prompt, modelName);

            // Assert
            Assert.Equal(mockResponse, result);
        }


        [Fact]
        public void GetOllamaApiTags_ShouldReturnKeyValuePairList_WhenApiCallIsSuccessful()
        {
            // Arrange
            var mockResponse = JsonBuilder.CreateAnonymousObject(JsonBuilder.CreateField("tag", "mockTag"));
            _mockApiClient.Setup(client => client.GetOllamaApiTags()).Returns(mockResponse);

            // Act
            var result = _mockApiClient.Object.GetOllamaApiTags();

            // Assert
            Assert.Equal(mockResponse, result);
        }
    }
}

