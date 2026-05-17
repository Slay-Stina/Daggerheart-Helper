using Core.Enums;

namespace Core.Entities;

public class FeatureEffect
{
    public Guid Id { get; init; }
    public Guid FeatureId { get; init; }
    public Feature Feature { get; init; } = null!;

    public FeatureEffectType EffectType { get; init; }
    public FeatureEffectTarget Target { get; init; }
    public FeatureEffectTiming Timing { get; init; } = FeatureEffectTiming.Passive;

    public TraitType? TraitType { get; init; }
    public int? IntValue { get; init; }
    public string? Condition { get; init; }
    public string? Description { get; init; }
}

