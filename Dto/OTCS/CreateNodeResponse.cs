using Newtonsoft.Json;

namespace SPCoEdit.Dto.OTCS
{
    public class CreateNodeResponse : CommonOTCSResponse
    {
        [JsonProperty("id")]
        public long ID { get; set; }
    }
}
