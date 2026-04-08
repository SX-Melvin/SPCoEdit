using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace SPCoEdit.Dto.OTCS
{
    public class SearchResponse: CommonOTCSResponse
    {
        [JsonPropertyName("results")]
        public List<SearchResponseResult> Results { get; set; } = new();
    }

    public class SearchResponseResult
    {
        [JsonPropertyName("data")]
        public SearchResponseData Data { get; set; } = default!;
    }

    public class SearchResponseData
    {
        [JsonPropertyName("properties")]
        public SearchResponseDataProperties Properties { get; set; } = default!;

        [JsonPropertyName("versions")]
        public SearchResponseDataVersion Versions { get; set; }
    }
    public class SearchResponseDataVersion
    {
        [JsonProperty("version_number")]
        public int VersionNumber { get; set; }
        
        [JsonProperty("version_number_major")]
        public int VersionNumberMajor { get; set; }
        
        [JsonProperty("version_number_minor")]
        public int VersionNumberMinor { get; set; }
        
        [JsonProperty("version_number_name")]
        public string? VersionNumberName { get; set; }
    }
    public class SearchResponseDataProperties
    {
        [JsonPropertyName("container")]
        public bool Container { get; set; }

        [JsonPropertyName("create_date")]
        public string? CreateDate { get; set; }
        
        [JsonPropertyName("create_user_id")]
        public int CreateUserId { get; set; }
        
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        
        [JsonPropertyName("favorite")]
        public bool Favorite { get; set; }
        
        [JsonPropertyName("id")]
        public long Id { get; set; }
        
        [JsonPropertyName("mime_type")]
        public string? mimeType { get; set; }
        
        [JsonPropertyName("modify_date")]
        public DateTime? ModifyDate { get; set; }
        
        [JsonPropertyName("modify_user_id")]
        public int ModifyUserId { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("owner")]
        public string? Owner { get; set; }
        
        [JsonPropertyName("owner_group_id")]
        public int OwnerGroupId { get; set; }
        
        [JsonPropertyName("owner_user_id")]
        public int OwnerUserId { get; set; }
        
        [JsonPropertyName("parent_id")]
        public int ParentId { get; set; }
        
        [JsonPropertyName("reserved")]
        public bool Reserved { get; set; }
        
        [JsonPropertyName("reserved_date")]
        public DateTime? ReservedDate { get; set; }
        
        [JsonPropertyName("reserved_user_id")]
        public int ReservedUserId { get; set; }
        
        [JsonPropertyName("size")]
        public long Size { get; set; }
        
        [JsonPropertyName("Type")]
        public int Type { get; set; }
        
        [JsonPropertyName("type_name")]
        public string? TypeName { get; set; }
    }
}
