using System.Text.Json.Serialization;

namespace Srd.Ingestion.Raw;

public sealed class RawFeatureDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("question")]
    public string? Question { get; set; }
}

