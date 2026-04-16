using Core.Enums;
using Core.Value_Objects;

namespace Core.Entities;

public class Armor
{
    public int Id { get; set; }
    public Tier Tier { get; set; }
    public string Name { get; set; } = string.Empty;
    public DamageThresholds DamageThresholds { get; set; } = new(0,0);
    public int ArmorScore { get; set; }
    public Feature? Feature { get; set; }
}