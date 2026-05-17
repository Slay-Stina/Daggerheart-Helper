using Core.ValueObjects;

namespace Core.Entities;

public class Armor
{
    public Guid Id { get; init; }
    public int Tier { get; init; }
    public string Name { get; init; } = string.Empty;
    public DamageThresholds DamageThresholds { get; init; } = new(0,0);
    public int ArmorScore { get; init; }
    
    public Guid? FeatureId { get; init; }
    public Feature? Feature { get; set; }
}
