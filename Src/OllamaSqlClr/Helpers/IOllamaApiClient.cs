using System.Collections.Generic;

namespace OllamaSqlClr.Helpers
{
    public interface IOllamaApiClient
    {
        List<KeyValuePair<string, object>> GetModelResponseToPrompt(string prompt, string modelName);
        List<KeyValuePair<string, object>> GetModelResponseToPrompt(string prompt, string modelName, List<int> context);
        List<KeyValuePair<string, object>> GetOllamaApiTags();
    }
}

