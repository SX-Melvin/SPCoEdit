using Newtonsoft.Json;

namespace SPCoEdit.Dto.OTCS
{
    public class CreateNodeRequest
    {
        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("parent_id")]
        public long ParentID { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("otcsTicket")]
        public string? OTCSTicket { get; set; } = null;

        [JsonProperty("token")]
        public string? Token { get; set; } = null;
    }
}
