using System.Text.Json.Serialization;

namespace Srd.Ingestion.Raw;

public sealed class RawSubclassDto
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("foundation")]
    public RawFeatureDto[] Foundation { get; set; } = null!;

    [JsonPropertyName("mastery")]
    public RawFeatureDto[] Mastery { get; set; } = null!;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("specialization")]
    public RawFeatureDto[] Specialization { get; set; } = null!;
    
    [JsonPropertyName("spellcast_trait")]
    public string? SpellcastTrait { get; set; } = string.Empty;
}