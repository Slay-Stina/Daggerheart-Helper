using Srd.Ingestion.Loading;
using Xunit;

namespace DaggerheartHelper.Tests.Srd.Ingestion.Tests.Loading;

public class SrdJsonLoaderTests
{
    [Fact]
    public async Task LoadAsync_ParsesKnownEntries()
    {
        var loader = new SrdJsonLoader();
        var catalog = await loader.LoadAsync(TestRepoPaths.SrdJsonDirectory);

        Assert.NotEmpty(catalog.Armors);
        Assert.NotEmpty(catalog.Weapons);
        Assert.NotEmpty(catalog.Abilities);

        var gambeson = Assert.Single(catalog.Armors, x => x.Name == "Gambeson Armor");
        Assert.Equal(1, gambeson.Tier);
        Assert.Equal(3, gambeson.ArmorScore);
        Assert.Equal(new Core.ValueObjects.DamageThresholds(5, 11), gambeson.DamageThresholds);
        Assert.Single(gambeson.Features);
        Assert.Equal("Flexible", gambeson.Features[0].Name);

        var broadsword = Assert.Single(catalog.Weapons, x => x.Name == "Broadsword");
        Assert.Equal(1, broadsword.Tier);
        Assert.Equal(Core.Enums.Burden.OneHanded, broadsword.Burden);
        Assert.Equal(Core.Enums.WeaponPriority.Primary, broadsword.Priority);
        Assert.Equal(Core.Enums.TraitType.Agility, broadsword.Trait);
        Assert.Equal(Core.Enums.RangeType.Melee, broadsword.RangeType);
        Assert.Equal(new Core.ValueObjects.Damage(new Core.ValueObjects.Dice(1, 8), 0, Core.Enums.DamageType.Physical), broadsword.Damage);

        var runeWard = Assert.Single(catalog.Abilities, x => x.Name == "Rune Ward");
        Assert.Equal(Core.Enums.DomainType.Arcana, runeWard.Domain);
        Assert.Equal(1, runeWard.Level);
        Assert.Equal(0, runeWard.RecallCost);
        Assert.Equal(Core.Enums.AbilityType.Spell, runeWard.Type);
    }

    [Fact]
    public async Task LoadAsync_ParsesKnownEntriesFromExternalSrd_WhenPresent()
    {
        if (!TestRepoPaths.HasExternalSrdJson)
        {
            return;
        }

        var loader = new SrdJsonLoader();
        var catalog = await loader.LoadAsync(TestRepoPaths.SrdJsonDirectory);

        Assert.NotEmpty(catalog.Armors);
        Assert.NotEmpty(catalog.Weapons);
        Assert.NotEmpty(catalog.Abilities);
    }
}


