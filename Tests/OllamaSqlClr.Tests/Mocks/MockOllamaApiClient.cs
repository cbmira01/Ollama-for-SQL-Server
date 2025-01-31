﻿using OllamaSqlClr.Helpers;
using System.Collections.Generic;
using System.Threading;
using Configuration;

namespace OllamaSqlClr.Tests.Mocks
{
    public class MockOllamaApiClient : IOllamaApiClient
    {
        private readonly string _generateEndpointUrl = AppConfig.GenerateEndpointUrl;
        private readonly string _tagEndpointUrl = AppConfig.TagEndpointUrl;
        private readonly int _timeout = AppConfig.ApiTimeoutMs;

        public MockOllamaApiClient() {}

        public List<KeyValuePair<string, object>> GetModelResponseToPrompt(string prompt, string modelName)
        {
            return new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("response", $"Mocked response for model '{modelName}' and prompt '{prompt}'"),
                new KeyValuePair<string, object>("modelName", modelName),
                new KeyValuePair<string, object>("prompt", prompt)
            };
        }

        public List<KeyValuePair<string, object>> GetModelResponseToPrompt(string prompt, string modelName, List<int> context)
        {
            return new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("response", $"Mocked response with context for model '{modelName}' and prompt '{prompt}'"),
                new KeyValuePair<string, object>("modelName", modelName),
                new KeyValuePair<string, object>("prompt", prompt),
                new KeyValuePair<string, object>("context", context != null ? string.Join(",", context) : "null")
            };
        }

        public List<KeyValuePair<string, object>> GetOllamaApiTags()
        {
            return new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("name", "mockModel"),
                new KeyValuePair<string, object>("model", "mockModel:v1"),
                new KeyValuePair<string, object>("modified_at", "2024-01-01T00:00:00Z"),
                new KeyValuePair<string, object>("size", 1024),
                new KeyValuePair<string, object>("family", "mock"),
                new KeyValuePair<string, object>("parameter_size", "large"),
                new KeyValuePair<string, object>("quantization_level", "high"),
                new KeyValuePair<string, object>("digest", "abcd1234")
            };
        }

        public List<KeyValuePair<string, object>> GetModelResponseToImage(string prompt, string modelName, string base64Image)
        {
            return new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("response", $"Mocked response for model '{modelName}' and prompt '{prompt}'"),
                new KeyValuePair<string, object>("modelName", modelName),
                new KeyValuePair<string, object>("prompt", prompt)
            };
        }
    }
}

