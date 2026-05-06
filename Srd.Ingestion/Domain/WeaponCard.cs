using Core.Enums;
using Core.ValueObjects;

namespace Srd.Ingestion.Domain;

public sealed record WeaponCard(
    string Name,
    int Tier,
    Burden Burden,
    WeaponPriority Priority,
    TraitType Trait,
    RangeType RangeType,
    Damage Damage,
    FeatureBlock? Feature);


