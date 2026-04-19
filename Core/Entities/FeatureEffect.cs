using Core.Enums;

namespace Core.Entities;

public class FeatureEffect
{
    public int Id { get; set; }
    public int FeatureId { get; set; }
    public Feature Feature { get; set; } = null!;

    public FeatureEffectType EffectType { get; set; }
    public FeatureEffectTarget Target { get; set; }
    public FeatureEffectTiming Timing { get; set; } = FeatureEffectTiming.Passive;

    public TraitType? TraitType { get; set; }
    public int? IntValue { get; set; }
    public string? Condition { get; set; }
    public string? Description { get; set; }
}

