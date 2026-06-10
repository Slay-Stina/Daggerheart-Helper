using Core.ValueObjects;

namespace Core.Entities;

public class Character
{
    public Guid Id { get; set; }
    public int Level { get; set; } = 1;
    public string Name { get; set; } = string.Empty;
    public string? Pronouns { get; set; }

    // Character Description
    public string? DescriptionEyes { get; set; }
    public string? DescriptionBody { get; set; }
    public string? DescriptionClothes { get; set; }
    public string? DescriptionSkin { get; set; }
    public string? DescriptionAttitude { get; set; }

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

    public TraitScores Traits { get; set; } = new(0, 0, 0, 0, 0, 0);
    public DamageThresholds DamageThresholds { get; set; } = new(0, 0);
    public int Evasion { get; set; }
    public int ProficiencyBonus { get; set; }
    public int Proficiency => 1 + (Level >= 2 ? 1 : 0) + (Level >= 5 ? 1 : 0) + (Level >= 8 ? 1 : 0) + ProficiencyBonus;

    public List<string> Experiences { get; set; } = new();
    public Dictionary<string, string> BackgroundAnswers { get; set; } = new();
    public List<Item> Inventory { get; set; } = new();
    public int GoldHandfuls { get; set; } = 1;
    public string? SpellFocus { get; set; }

    public Guid? EquippedArmorId { get; set; }
    public Armor? EquippedArmor { get; set; }
    public Guid? PrimaryWeaponId { get; set; }
    public Weapon? PrimaryWeapon { get; set; }
    public Guid? SecondaryWeaponId { get; set; }
    public Weapon? SecondaryWeapon { get; set; }
    public List<CharacterAbility> CharacterAbilities { get; set; } = new();
    public ResourcePool HitPoints { get; set; } = new(5, 5);
    public ResourcePool Stress { get; set; } = new(0, 5);
    public ResourcePool Hope { get; set; } = new(0, 5);
    public ResourcePool ArmorSlots { get; set; } = new(0, 0);
    public byte[] RowVersion { get; set; } = [];
}
