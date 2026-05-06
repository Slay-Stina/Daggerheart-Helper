using Core.Enums;
using Core.ValueObjects;
using Srd.Ingestion.Domain;
using Srd.Ingestion.Mapping;
using Xunit;

namespace DaggerheartHelper.Tests.Srd.Ingestion.Tests.Mapping;

public class SrdEntityMapperTests
{
    [Fact]
    public void ToEntity_MapsArmorCardToArmorEntity()
    {
        var card = new ArmorCard(
            "Gambeson Armor",
            1,
            3,
            new DamageThresholds(5, 11),
            new FeatureBlock("Flexible", "+1 to Evasion"));

        var entity = card.ToEntity();

        Assert.Equal("Gambeson Armor", entity.Name);
        Assert.Equal(1, entity.Tier);
        Assert.Equal(3, entity.ArmorScore);
        Assert.Equal(new DamageThresholds(5, 11), entity.DamageThresholds);
        Assert.Equal("Flexible", entity.Feature?.Name);
    }

    [Fact]
    public void ToEntity_MapsAbilityCardToAbilityEntity()
    {
        var card = new AbilityCard("Rune Ward", DomainType.Arcana, 1, 0, AbilityType.Spell, "Text");

        var entity = card.ToEntity();

        Assert.Equal("Rune Ward", entity.Title);
        Assert.Equal(DomainType.Arcana, entity.DomainType);
        Assert.Equal(1, entity.Level);
        Assert.Equal(0, entity.RecallCost);
        Assert.Equal(AbilityType.Spell, entity.Type);
        Assert.Equal("Text", entity.FeatureDescription);
    }

    [Fact]
    public void ToEntity_MapsClassCardToClassEntity()
    {
        var feature = new FeatureBlock("FeatureName", "FeatureText");
        var stringlist = new List<string> { "string1", "string2", "string3" };
        var card = new ClassCard("Bard", "ClassDescription",
            DomainType.Grace, DomainType.Codex, 10, 5,
            new TraitScores(0, 0, -1, 1, 1, 2),
            new List<SubclassCard>(), feature, feature,
            stringlist, stringlist, stringlist, 
            new ArmorCard("ArmorName",1,1, 
                new DamageThresholds(1,2),null),
            new List<WeaponCard>());
        
        var entity = card.ToEntity();
        
        Assert.Equal("Bard", entity.Name);
        Assert.Equal("ClassDescription", entity.Description);
        Assert.Equal(DomainType.Grace, entity.Domain1);
        Assert.Equal(10, entity.BaseHealth);
        Assert.Equal("ArmorName", entity.SuggestedArmor.Name);
        Assert.Null(entity.SuggestedArmor.Feature);
    }

    [Fact]
    public void ToEntity_MapsSubclassCardToSubclassEntity()
    {
        var feature = new FeatureBlock("FeatureName", "FeatureText");
        var wordsmith = new SubclassCard(
            "Wordsmith", 
            "SubclassDescription",
            TraitType.Presence,
            feature, feature, feature);
        
        var entity  = wordsmith.ToEntity();
        Assert.Equal("Wordsmith", entity.Name);
        Assert.Equal("SubclassDescription", entity.Description);
        Assert.Equal(TraitType.Presence, entity.SpellCastingTraitType);
        Assert.Equal("FeatureName", entity.Foundation.Name);
    }

    [Fact]
    public void ToEntity_MapsAncestryAndCommunitCardsToHeritageEntity()
    {
        var ancestry = new AncestryCard(
            "AncestryName",
            "AncestryDescription",
            new List<FeatureBlock>(),
            HeritageType.Ancestry);

        var community = new CommunityCard(
            "CommunityName",
            "CommunityDescription",
            new List<FeatureBlock>(),
            "Note",
            HeritageType.Community);

        var ancestryEntity = ancestry.ToEntity();
        var communityEntity = community.ToEntity();

        Assert.NotNull(ancestryEntity);
        Assert.NotNull(communityEntity);
        Assert.Equal("AncestryName", ancestryEntity.Name);
        Assert.Equal("AncestryDescription", ancestryEntity.Description);
        Assert.Equal(HeritageType.Ancestry, ancestryEntity.HeritageType);
        Assert.Equal(HeritageType.Community, communityEntity.HeritageType);
        Assert.Empty(ancestryEntity.Features);
        Assert.Equal(community.Note, communityEntity.Note);
        Assert.Equal(ancestryEntity.GetType(), communityEntity.GetType());
    }

    [Fact]
    public void ToEntity_MapsWeaponCardToWeaponEntity()
    {
        var weapon = new WeaponCard(
            "Broadsword",
            1,
            Burden.OneHanded,
            WeaponPriority.Primary,
            TraitType.Agility,
            RangeType.Melee,
            new Damage(new Dice(1, 8), 0, DamageType.Physical), 
            null
        );
        
        var entity = weapon.ToEntity();
        
        Assert.Equal("Broadsword", entity.Name);
        Assert.Equal(1, entity.Tier);
    }
}



