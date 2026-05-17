using Core.Enums;
using Core.ValueObjects;

namespace Core.Entities;

public class Weapon
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public TraitType Trait { get; init; }
    public required Damage Damage { get; init; }
    public Burden Burden { get; init; }
    public RangeType RangeType { get; init; }
    public WeaponPriority Category { get; init; }
    
    public Guid? FeatureId { get; init; }
    public Feature? Feature { get; set; }
    
    public int Tier { get; init; }
}
