using System.Text.Json.Serialization;

namespace SPCoEdit.Dto.OTCS
{
    public class SearchRequest
    {
        [JsonPropertyName("where")]
        public required string Where { get; set; }

        [JsonPropertyName("lookfor")]
        public string Lookfor { get; set; } = "anywords";

        [JsonPropertyName("page")]
        public int Page { get; set; } = 1;

        [JsonPropertyName("filter")]
        public string Filter { get; set; } = "OTSubType:{749|144}"; // Document and email

        [JsonPropertyName("limit")]
        public int Limit { get; set; } = 10;
    }
}
