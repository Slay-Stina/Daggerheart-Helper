using Core.Enums;
using Core.Value_Objects;
using Range = Core.Enums.Range;

namespace Core.Entities;

public class Weapon
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TraitType Trait { get; set; }
    public required Damage Damage { get; set; }
    public Burden Burden { get; set; }
    public Range Range { get; set; }
    public WeaponPriority Category { get; set; }
    public List<Feature> Features { get; set; } = new();
    public Tier Tier { get; set; }
}