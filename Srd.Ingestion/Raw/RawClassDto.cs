using System.Text.Json.Serialization;

namespace Srd.Ingestion.Raw;

public sealed class RawClassDto
{
    [JsonPropertyName("background")]
    public List<Question> BackgroundQuestions { get; set; } = null!;

    [JsonPropertyName("connections")]
    public List<Question> ConnectionQuestions { get; set; } = null!;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("domain_1")]
    public string Domain1 { get; set; } = string.Empty;

    [JsonPropertyName("domain_2")]
    public string Domain2 { get; set; } = string.Empty;

    [JsonPropertyName("evasion")]
    public string BaseEvasion { get; set; } = string.Empty;

    [JsonPropertyName("feature")]
    public List<RawFeatureDto> Features { get; set; } = new();

    [JsonPropertyName("hope_feature_name")]
    public string  HopeFeatureName { get; set; } = string.Empty;
    
    [JsonPropertyName("hope_feature_text")]
    public string HopeFeatureText { get; set; } = string.Empty;
    
    [JsonPropertyName("hp")]
    public string BaseHp { get; set; } =  string.Empty;
    
    [JsonPropertyName("items")]
    public string Items { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("subclass_1")]
    public string SubClass1 { get; set; } = string.Empty;
    
    [JsonPropertyName("subclass_2")]
    public string SubClass2 { get; set; } = string.Empty;

    [JsonPropertyName("suggested_armor")]
    public string SuggestedArmor { get; set; } = string.Empty;
    
    [JsonPropertyName("suggested_primary")]
    public string SuggestedPrimary { get; set; } = string.Empty;
    
    [JsonPropertyName("suggested_secondary")]
    public string SuggestedSecondary { get; set; } = string.Empty;

    [JsonPropertyName("suggested_traits")]
    public string SuggestedTraits { get; set; } = string.Empty;
}

public class Question
{
    public string Text { get; set; } = string.Empty;
}