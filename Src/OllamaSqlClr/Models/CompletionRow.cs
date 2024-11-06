using System;

namespace OllamaSqlClr.Models
{
    // Describe a table row when completing multiple prompts
    public class CompletionRow
    {
        public Guid CompletionGuid { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public string OllamaCompletion { get; set; } = string.Empty;
    }
}
