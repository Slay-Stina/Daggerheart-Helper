using System.Text.Json.Serialization;

namespace Srd.Ingestion.Raw;

public sealed class RawCommunityDto
{
    [JsonPropertyName("description")]
    public string Description { get; set; }
    
    [JsonPropertyName("feature")]
    public List<RawFeatureDto> Feature { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("note")]
    public string Note { get; set; }
}