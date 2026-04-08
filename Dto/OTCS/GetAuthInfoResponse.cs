using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace SPCoEdit.Dto.OTCS
{
    public class GetAuthInfoResponse: CommonOTCSResponse
    {
        public GetAuthInfoResponseData Data { get; set; }
    }

    public class GetAuthInfoResponseData
    {
        [JsonProperty("id")]
        public long ID { get; set; }

        [JsonProperty("deleted")]
        public bool Deleted { get; set; }

        [JsonProperty("first_name")]
        public string? FirstName { get; set; } = null;
        
        [JsonProperty("last_name")]
        public string? LastName { get; set; } = null;

        [JsonProperty("personal_email")]
        public string? PersonalEmail { get; set; } = null;

        [JsonProperty("photo_url")]
        public string? PhotoUrl { get; set; } = null;

        [JsonProperty("middle_name")]
        public string? MiddleName { get; set; } = null;

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
