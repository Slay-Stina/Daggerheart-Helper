using Core.ValueObjects;

namespace Application.Dtos;

public sealed record ArmorSummary(
    Guid Id,
    string Name,
    int Tier,
    int ArmorScore,
    DamageThresholds DamageThresholds,
    FeatureSummary? Feature);
