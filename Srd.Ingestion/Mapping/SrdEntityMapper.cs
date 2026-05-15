using Core.Entities;
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
            Tier = card.Tier,
            ArmorScore = card.ArmorScore,
            DamageThresholds = card.DamageThresholds,
            Feature = ToEntity(card.Feature)
        };
    }

    public static Weapon ToEntity(this WeaponCard card)
    {
        return new Weapon
        {
            Name = card.Name,
            Tier = card.Tier,
            Trait = card.Trait,
            Burden = card.Burden,
            RangeType = card.RangeType,
            Category = card.Priority,
            Damage = card.Damage,
            Feature = ToEntity(card.Feature)
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
            Features = card.Features.Select(ToEntity).ToList()!,
            Note = card.Note,
            HeritageType = card.HeritageType
        };
    }

    public static Subclass ToEntity(this SubclassCard card)
    {
        // generate id early so features can reference subclass
        var id = Guid.NewGuid();
        var foundation = ToEntity(card.Foundation)!;
        var specialization = ToEntity(card.Specialization)!;
        var mastery = ToEntity(card.Mastery)!;


        return new Subclass
        {
            Id = id,
            Name = card.Name,
            Description = card.Description,
            Foundation = foundation,
            Specialization = specialization,
            Mastery = mastery,
            SpellCastingTraitType = card.SpellcastTrait
        };
    }

    public static GameClass ToEntity(this ClassCard card)
    {
        var id = Guid.NewGuid();

        var classFeatures = card.ClassFeatures.Select(f => ToEntity(f)!).ToList();
        foreach (var cf in classFeatures)
            cf.GameClassIdAsClassFeature = id;

        var hopeFeature = ToEntity(card.HopeFeature)!;

        return new GameClass
        {
            Id = id,
            Name = card.Name,
            Description = card.Description,
            BaseEvasion = card.BaseEvasion,
            BaseHealth = card.BaseHp,
            Domain1 = card.Domain1,
            Domain2 = card.Domain2,
            SuggestedTraits = card.SuggestedTraitScores,
            SuggestedArmor = null,
            SuggestedWeapons = new List<Weapon>(),
            Subclasses = card.Subclasses.Select(ToEntity).ToList(),
            ClassFeatures = classFeatures,
            HopeFeature = hopeFeature,
            BackgroundQuestions = card.BackgroundQuestions,
            ConnectionQuestions = card.ConnectionQuestions,
            Items = card.Items,
        };
    }
    
    public static GameClass ToEntity(this ClassCard card, Dictionary<string, Armor> armorByName, Dictionary<string, Weapon> weaponsByName)
    {
        var id = Guid.NewGuid();

        var classFeatures = card.ClassFeatures.Select(f => ToEntity(f)!).ToList();
        foreach (var cf in classFeatures)
            cf.GameClassIdAsClassFeature = id;

        var hopeFeature = ToEntity(card.HopeFeature)!;

        return new GameClass
        {
            Id = id,
            Name = card.Name,
            Description = card.Description,
            BaseEvasion = card.BaseEvasion,
            BaseHealth = card.BaseHp,
            Domain1 = card.Domain1,
            Domain2 = card.Domain2,
            SuggestedTraits = card.SuggestedTraitScores,
            SuggestedArmor = armorByName.TryGetValue(card.SuggestedArmor.Name, out var armor) ? armor : null,
            SuggestedWeapons = card.SuggestedWeapons
                .Where(w => weaponsByName.ContainsKey(w.Name))
                .Select(w => weaponsByName[w.Name])
                .ToList(),
            Subclasses = card.Subclasses.Select(ToEntity).ToList(),
            ClassFeatures = classFeatures,
            HopeFeature = hopeFeature,
            BackgroundQuestions = card.BackgroundQuestions,
            ConnectionQuestions = card.ConnectionQuestions,
            Items = card.Items,
        };
    }
    
    private static Feature? ToEntity(FeatureBlock? feature)
    {
        return feature is null ? null :
            new Feature
        {
            Name = feature.Name,
            Description = feature.Text
        };
    }
}

