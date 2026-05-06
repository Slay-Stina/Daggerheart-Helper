using System.Text.Json.Serialization;

namespace Srd.Ingestion.Raw;

public sealed class RawWeaponDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("tier")]
    public string Tier { get; set; } = string.Empty;

    [JsonPropertyName("burden")]
    public string Burden { get; set; } = string.Empty;

    [JsonPropertyName("damage")]
    public string Damage { get; set; } = string.Empty;

    [JsonPropertyName("trait")]
    public string Trait { get; set; } = string.Empty;

    [JsonPropertyName("range")]
    public string Range { get; set; } = string.Empty;

    [JsonPropertyName("primary_or_secondary")]
    public string PrimaryOrSecondary { get; set; } = string.Empty;

    [JsonPropertyName("physical_or_magical")]
    public string PhysicalOrMagical { get; set; } = string.Empty;

    [JsonPropertyName("feature")]
    public List<RawFeatureDto>? Feature { get; set; }
}

