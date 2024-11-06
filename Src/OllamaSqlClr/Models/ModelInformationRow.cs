using System;

namespace OllamaSqlClr.Models
{
    // Describe items responding from the /api/tags out
    public class ModelInformationRow
    {
        public Guid ModelGuid { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public string ReferToName { get; set; }
        public DateTime ModifiedAt { get; set; }
        public long Size { get; set; }
        public string Family { get; set; }
        public string ParameterSize { get; set; }
        public string QuantizationLevel { get; set; }
        public string Digest { get; set; }
    }
}
