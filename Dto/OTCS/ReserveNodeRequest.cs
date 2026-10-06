using Newtonsoft.Json;

namespace SPCoEdit.Dto.OTCS
{
    public class ReserveNodeRequest
    {
        [JsonProperty("reserved_user_id")]
        public long? ReservedUserID { get; set; } = null;
    }
}
