using Core.ValueObjects;

namespace Core.Entities;

public class Character
{
    public Guid Id { get; set; }
    public int Level { get; set; } = 1;
    public string Name { get; set; } = string.Empty;

    public Guid GameClassId { get; set; }
    public GameClass GameClass { get; set; } = null!;
    public Guid SubclassId { get; set; }
    public Subclass Subclass { get; set; } = null!;
    public Guid? MulticlassId { get; set; }
    public GameClass? Multiclass { get; set; }
    public Guid? MulticlassSubclassId { get; set; }
    public Subclass? MulticlassSubclass { get; set; }

    public Guid AncestryId { get; set; }
    public Heritage Ancestry { get; set; } = null!;
    public Guid CommunityId { get; set; }
    public Heritage Community { get; set; } = null!;
    
    public TraitScores Traits { get; set; } = new(0, 0,0,0,0,0);
    public DamageThresholds DamageThresholds { get; set; } = new(0,0,0);
    public int Evasion { get; private set; }
    public int ProficiencyBonus { get; set; }
    public int Proficiency => (Level + 2) / 3;

    public int? EquippedArmorId { get; set; }
    public Armor? EquippedArmor { get; set; }
    public int? PrimaryWeaponId { get; set; }
    public Weapon? PrimaryWeapon { get; set; }
    public int? SecondaryWeaponId { get; set; }
    public Weapon? SecondaryWeapon { get; set; }
    
    public ResourcePool HitPoints { get; set; } = new(5, 5);
    public ResourcePool Stress { get; set; } = new(0, 5);
    public ResourcePool Hope { get; set; } = new(0, 5);
    public ResourcePool ArmorSlots { get; set; } = new(0, 5);

}
