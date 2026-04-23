using Core.Enums;
using Core.ValueObjects;
using Range = Core.Enums.Range;

namespace Srd.Ingestion.Domain;

public sealed record WeaponCard(
    string Name,
    int Tier,
    Burden Burden,
    WeaponPriority Priority,
    TraitType Trait,
    Range Range,
    DamageType DamageKind,
    Damage Damage,
    IReadOnlyList<FeatureBlock> Features);


