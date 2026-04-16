using Core.Enums;
using Core.Value_Objects;

namespace Core.Entities;

public class Character
{
    public Guid Id { get; set; }
    public int Level { get; set; } = 1;
    public string Name { get; set; } = string.Empty;

    public int GameClassId { get; set; }
    public GameClass Class { get; set; } = null!;
    public Subclass Subclass { get; set; } = null!;
    public GameClass? Multiclass { get; set; }
    public Subclass? MulticlassSubclass { get; set; }
    
    public TraitScores Traits { get; set; } = new(0, 0,0,0,0,0);
    public DamageThresholds DamageThresholds { get; set; } = new(0,0,0);
    public int Evasion { get; private set; }
    public int ProficiencyBonus { get; set; }
    public int Proficiency => (Level + 2) / 3;

    public Armor? EquippedArmor { get; set; }
    public List<Weapon>? Weapon { get; set; }
    
    public ResourcePool HitPoints { get; set; } = new(5, 5);
    public ResourcePool Stress { get; set; } = new(0, 5);
    public ResourcePool Hope { get; set; } = new(0, 5);
    public ResourcePool ArmorSlots { get; set; } = new(0, 5);

}