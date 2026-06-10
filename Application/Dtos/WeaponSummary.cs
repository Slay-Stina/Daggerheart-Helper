using Core.Enums;
using Core.ValueObjects;

namespace Application.Dtos;

public sealed record WeaponSummary(
    Guid Id,
    string Name,
    int Tier,
    Damage Damage,
    Burden Burden,
    RangeType RangeType,
    TraitType Trait,
    WeaponPriority Category,
    FeatureSummary? Feature = null);
