using System.Text.Json.Serialization;

namespace Srd.Ingestion.Raw;

public sealed class RawCommunityDto
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("feature")]
    public List<RawFeatureDto> Feature { get; set; } = null!;
    
    [JsonPropertyName("name")]
    public string Name { get; set; }  = string.Empty;
    
    [JsonPropertyName("note")]
    public string Note { get; set; }  = string.Empty;
}