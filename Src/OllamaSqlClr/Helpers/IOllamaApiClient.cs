using System.Collections.Generic;

namespace OllamaSqlClr.Helpers
{
    public interface IOllamaApiClient
    {
        List<KeyValuePair<string, object>> GetModelResponseToPrompt(string prompt, string modelName);

        List<KeyValuePair<string, object>> GetModelResponseToPrompt(string prompt, string modelName, List<int> context);

        List<KeyValuePair<string, object>> GetOllamaApiTags();

        List<KeyValuePair<string, object>> GetModelResponseToImage(string prompt, string modelName, string base64Image);

    }
}

