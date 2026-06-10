using Application.Dtos;
using Core.Entities;

namespace Infrastructure.Extensions;

public static class MappingExtensions
{
    public static Character ToNewCharacter(this CharacterSummary summary)
    {
        return new Character
        {
            Id = Guid.NewGuid(),
            Name = summary.Name,
            Pronouns = summary.Pronouns,
            DescriptionEyes = summary.DescriptionEyes,
            DescriptionBody = summary.DescriptionBody,
            DescriptionClothes = summary.DescriptionClothes,
            DescriptionSkin = summary.DescriptionSkin,
            DescriptionAttitude = summary.DescriptionAttitude,
            Level = summary.Level,
            GameClassId = summary.Class.Id,
            SubclassId = summary.Subclass.Id,
            AncestryId = summary.Ancestry.Id,
            CommunityId = summary.Community.Id,
            EquippedArmorId = summary.EquippedArmor?.Id,
            PrimaryWeaponId = summary.PrimaryWeapon?.Id,
            SecondaryWeaponId = summary.SecondaryWeapon?.Id,
            Traits = summary.Traits,
            DamageThresholds = summary.DamageThresholds,
            Evasion = summary.Evasion,
            Experiences = summary.Experiences.ToList(),
            BackgroundAnswers = summary.BackgroundAnswers,
            GoldHandfuls = summary.GoldHandfuls,
            SpellFocus = summary.SpellFocus,
            HitPoints = summary.HitPoints,
            Stress = summary.Stress,
            Hope = summary.Hope,
            ArmorSlots = summary.ArmorSlots,
            Inventory = summary.Inventory.Select(i => new Item { Id = i.Id, Name = i.Name, Description = "" }).ToList(),
            CharacterAbilities = summary.CharacterAbilities.Select(a => new CharacterAbility
            {
                AbilityId = a.Id,
                IsVaulted = false,
            }).ToList(),
        };
    }

    public static CharacterSummary ToSummary(this Character c)
    {
        return new CharacterSummary(
            c.Id, 
            c.Level, 
            c.Name, 
            c.Pronouns,
            c.DescriptionEyes, 
            c.DescriptionBody, 
            c.DescriptionClothes, 
            c.DescriptionSkin, 
            c.DescriptionAttitude,
            c.GameClass.ToSummary() ?? throw new InvalidOperationException("Game class is null"), 
            c.Subclass.ToSummary() ?? throw new InvalidOperationException("Subclass is null"), 
            c.Multiclass?.ToSummary(), 
            c.MulticlassSubclass?.ToSummary(),
            c.Ancestry.ToSummary(),
            c.Community.ToSummary(),
            c.Traits,
            c.DamageThresholds, 
            c.Evasion, 
            c.Proficiency,
            c.Experiences,
            c.BackgroundAnswers,
            c.Inventory.Select(i => new ItemSummary(i.Id, i.Name, i.Description)).ToList(), 
            c.GoldHandfuls, 
            c.SpellFocus,
            c.EquippedArmor?.ToSummary(),
            c.PrimaryWeapon.ToSummary(),
            c.SecondaryWeapon.ToSummary(),
            c.CharacterAbilities.Select(ToSummary).ToList(),
            c.HitPoints, 
            c.Stress, 
            c.Hope, 
            c.ArmorSlots, 
            c.RowVersion
            );

    }

    private static ClassCardSummary? ToSummary(this GameClass? gc)
    {
        if (gc is null) return null;
        return new ClassCardSummary(
            gc.Id, 
            gc.Name, 
            gc.Description, 
            gc.Domain1, 
            gc.Domain2, 
            gc.BaseEvasion, 
            gc.BaseHealth, 
            gc.ClassFeatures.Select(f => new FeatureSummary(f.Id, f.Name, f.Description)).ToList(), 
            new FeatureSummary(gc.HopeFeature.Id, gc.HopeFeature.Name, gc.HopeFeature.Description));
    }

    private static SubclassSummary? ToSummary(this Subclass? sc)
    {
        if (sc is null) return null;
        return new SubclassSummary(
            sc.Id, 
            sc.Name, 
            sc.Description,
            new FeatureSummary(sc.Foundation.Id, sc.Foundation.Name, sc.Foundation.Description),
            new FeatureSummary(sc.Specialization.Id, sc.Specialization.Name, sc.Specialization.Description),
            new FeatureSummary(sc.Mastery.Id, sc.Mastery.Name, sc.Mastery.Description));
    }
    
    static HeritageSummary ToSummary(this Heritage h)
    {
        return new HeritageSummary(
            h.Id, 
            h.Name, 
            h.Description, 
            h.HeritageType, 
            h.Features.Select(f => new FeatureSummary(f.Id, f.Name, f.Description)).ToList());
    }
    
    static ArmorSummary? ToSummary(this Armor? a)
    {
        if (a is null) return null;
        return new ArmorSummary(
            a.Id, 
            a.Name, 
            a.Tier, 
            a.ArmorScore, 
            a.DamageThresholds, 
            a.Feature != null ? new FeatureSummary(a.Feature.Id, a.Feature.Name, a.Feature.Description) : null);
    }

    static WeaponSummary? ToSummary(this Weapon? w)
    {
        if (w is null) return null;
        return new WeaponSummary(
            w.Id,
            w.Name,
            w.Tier,
            w.Damage,
            w.Burden,
            w.RangeType,
            w.Trait,
            w.Category,
            w.Feature != null ? new FeatureSummary(w.Feature.Id, w.Feature.Name, w.Feature.Description) : null);
    }

    static AbilitySummary ToSummary(this CharacterAbility ca)
    {
        return new AbilitySummary(
            ca.AbilityId, 
            ca.Ability.Title, 
            ca.Ability.DomainType, 
            ca.Ability.Level,
            ca.Ability.RecallCost, 
            ca.Ability.Type, 
            ca.Ability.FeatureDescription, 
            ca.IsVaulted);
    }
    
}