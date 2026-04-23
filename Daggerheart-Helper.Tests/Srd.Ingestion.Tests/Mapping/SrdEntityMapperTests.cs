using Core.Enums;
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
            new Core.ValueObjects.DamageThresholds(5, 11),
            [new FeatureBlock("Flexible", "+1 to Evasion")]);

        var entity = card.ToEntity();

        Assert.Equal("Gambeson Armor", entity.Name);
        Assert.Equal(Tier.Tier1, entity.Tier);
        Assert.Equal(3, entity.ArmorScore);
        Assert.Equal(new Core.ValueObjects.DamageThresholds(5, 11), entity.DamageThresholds);
        Assert.Single(entity.Features);
        Assert.Equal("Flexible", entity.Features[0].Name);
    }

    [Fact]
    public void ToEntity_MapsAbilityCardToAbilityEntity()
    {
        var card = new AbilityCard("Rune Ward", Domain.Arcana, 1, 0, AbilityType.Spell, "Text");

        var entity = card.ToEntity();

        Assert.Equal("Rune Ward", entity.Title);
        Assert.Equal(Domain.Arcana, entity.Domain);
        Assert.Equal(1, entity.Level);
        Assert.Equal(0, entity.RecallCost);
        Assert.Equal(AbilityType.Spell, entity.Type);
        Assert.Equal("Text", entity.FeatureDescription);
    }
}



