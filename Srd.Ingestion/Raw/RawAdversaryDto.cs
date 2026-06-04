using System.Text.Json.Serialization;

namespace Srd.Ingestion.Raw;

public sealed class RawAdversaryDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    [JsonPropertyName("tier")]
    public string Tier { get; set; } = string.Empty;
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = string.Empty;
    [JsonPropertyName("hp")]
    public string Hp { get; set; } = string.Empty;
    [JsonPropertyName("stress")]
    public string Stress { get; set; } = string.Empty;
    [JsonPropertyName("thresholds")]
    public string Thresholds { get; set; } = string.Empty;
    [JsonPropertyName("atk")]
    public string Atk { get; set; } = string.Empty;
    [JsonPropertyName("attack")]
    public string Attack { get; set; } = string.Empty;
    [JsonPropertyName("damage")]
    public string Damage { get; set; } = string.Empty;
    [JsonPropertyName("range")]
    public string Range { get; set; } = string.Empty;
    [JsonPropertyName("experience")]
    public string Experience { get; set; } = string.Empty;
    [JsonPropertyName("motives_and_tactics")]
    public string MotivesAndTactics { get; set; } = string.Empty;
    [JsonPropertyName("feature")]
    public RawFeatureDto[]? Feature { get; set; }
}
