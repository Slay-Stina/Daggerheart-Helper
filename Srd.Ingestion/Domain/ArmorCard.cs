using Core.ValueObjects;

namespace Srd.Ingestion.Domain;

public sealed record ArmorCard(
    string Name,
    int Tier,
    int ArmorScore,
    DamageThresholds DamageThresholds,
    IReadOnlyList<FeatureBlock> Features);


