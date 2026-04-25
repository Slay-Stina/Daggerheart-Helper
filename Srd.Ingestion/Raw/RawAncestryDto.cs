using System.Text.Json.Serialization;

namespace Srd.Ingestion.Raw;

public sealed class RawAncestryDto
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("feature")]
    public List<RawFeatureDto>? Features { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}