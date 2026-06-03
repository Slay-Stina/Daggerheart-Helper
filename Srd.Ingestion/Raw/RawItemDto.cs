using System.Text.Json.Serialization;

namespace Srd.Ingestion.Raw;

public sealed class RawItemDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("roll")]
    public string Roll { get; set; } = string.Empty;
}
