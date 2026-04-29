using System.ComponentModel.DataAnnotations;
using Core.Entities;
using Core.Enums;
using Srd.Ingestion.Domain;

namespace Srd.Ingestion.Mapping;

public static class SrdEntityMapper
{
    public static List<Armor> ToEntities(this IEnumerable<ArmorCard> cards) => cards.Select(ToEntity).ToList();

    public static List<Weapon> ToEntities(this IEnumerable<WeaponCard> cards) => cards.Select(ToEntity).ToList();

    public static List<Ability> ToEntities(this IEnumerable<AbilityCard> cards) => cards.Select(ToEntity).ToList();

    public static List<Heritage> ToEntities(this IEnumerable<HeritageCard> cards) => cards.Select(ToEntity).ToList();
    
    public static Armor ToEntity(this ArmorCard card)
    {
        return new Armor
        {
            Name = card.Name,
            Tier = ParseTier(card.Tier),
            ArmorScore = card.ArmorScore,
            DamageThresholds = card.DamageThresholds,
            Features = card.Features.Select(ToEntity).ToList()
        };
    }

    public static Weapon ToEntity(this WeaponCard card)
    {
        return new Weapon
        {
            Name = card.Name,
            Tier = ParseTier(card.Tier),
            Trait = card.Trait,
            Burden = card.Burden,
            RangeType = card.RangeType,
            Category = card.Priority,
            Damage = card.Damage,
            Description = string.Empty,
            Features = card.Features.Select(ToEntity).ToList()
        };
    }

    public static Ability ToEntity(this AbilityCard card)
    {
        return new Ability
        {
            Title = card.Name,
            DomainType = card.Domain,
            Level = card.Level,
            RecallCost = card.RecallCost,
            Type = card.Type,
            FeatureDescription = card.Text
        };
    }

    public static Heritage ToEntity(this HeritageCard card)
    {
        return new Heritage
        {
            Name = card.Name,
            Description = card.Description,
            Features = card.Features.Select(ToEntity).ToList(),
            Note = card.Note,
            HeritageType = card.HeritageType
        };
    }

    public static Subclass ToEntity(this SubclassCard card)
    {
        return new Subclass
        {
            Name = card.Name,
            Description = card.Description,
            Features = card.Features.Select(ToEntity).ToList(),
            SpellCastingTraitType = card.SpellcastTrait
        };
    }

    public static GameClass ToEntity(this ClassCard card)
    {
        return new GameClass
        {
            Name = card.Name,
            Description = card.Description,
            BaseEvasion = card.BaseEvasion,
            BaseHealth = card.BaseHp,
            Domain1 = card.Domain1,
            Domain2 = card.Domain2,
            SuggestedTraits = null,
            SuggestedArmor = null,
            SuggestedWeapons = null,
            Subclasses = null,
            Features = card.Features.Select(ToEntity)
                .ToList(),
            BackgroundQuestions = null,
            ConnectionQuestions = null,
            Items = null,

        };
    }
    
    private static Feature ToEntity(FeatureBlock feature)
    {
        return new Feature
        {
            Name = feature.Name,
            Description = feature.Text
        };
    }

    private static Tier ParseTier(int tier) => tier switch
    {
        1 => Tier.Tier1,
        2 => Tier.Tier2,
        3 => Tier.Tier3,
        4 => Tier.Tier4,
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Tier must be between 1 and 4.")
    };
}

