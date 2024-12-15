using System;

namespace OllamaSqlClr.Models
{
    public class QueryFromPromptRow
    {
        public Guid QueryGuid { get; set; }
        public string ModelName { get; set; }
        public string Prompt { get; set; }
        public string ProposedQuery { get; set; }
        public string Result { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
