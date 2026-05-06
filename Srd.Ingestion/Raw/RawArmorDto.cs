using System.Text.Json.Serialization;

namespace Srd.Ingestion.Raw;

public sealed class RawArmorDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("tier")]
    public string Tier { get; set; } = string.Empty;

    [JsonPropertyName("base_score")]
    public string BaseScore { get; set; } = string.Empty;

    [JsonPropertyName("base_thresholds")]
    public string BaseThresholds { get; set; } = string.Empty;

    [JsonPropertyName("feature")]
    public List<RawFeatureDto>? Feature { get; set; }
}

