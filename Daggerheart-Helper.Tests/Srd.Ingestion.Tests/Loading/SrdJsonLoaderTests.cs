using Core.Enums;
using DaggerheartHelper.Tests.Srd.Ingestion.Tests.Models;
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
        Assert.NotEmpty(catalog.Subclasses);
        Assert.NotEmpty(catalog.Classes);
        Assert.NotEmpty(catalog.Ancestries);
        Assert.NotEmpty(catalog.Communities);

        var gambeson = Assert.Single(catalog.Armors, x => x.Name == "Gambeson Armor");
        Assert.Equal(1, gambeson.Tier);
        Assert.Equal(3, gambeson.ArmorScore);
        Assert.Equal(new(5, 11), gambeson.DamageThresholds);
        Assert.Equal("Flexible", gambeson.Feature?.Name);

        var broadsword = Assert.Single(catalog.Weapons, x => x.Name == "Broadsword");
        Assert.Equal(1, broadsword.Tier);
        Assert.Equal(Burden.OneHanded, broadsword.Burden);
        Assert.Equal(WeaponPriority.Primary, broadsword.Priority);
        Assert.Equal(TraitType.Agility, broadsword.Trait);
        Assert.Equal(RangeType.Melee, broadsword.RangeType);
        Assert.Equal(new(new(1, 8), 0, DamageType.Physical), broadsword.Damage);

        var runeWard = Assert.Single(catalog.Abilities, x => x.Name == "Rune Ward");
        Assert.Equal(DomainType.Arcana, runeWard.Domain);
        Assert.Equal(1, runeWard.Level);
        Assert.Equal(0, runeWard.RecallCost);
        Assert.Equal(AbilityType.Spell, runeWard.Type);
        
        var troubadour = Assert.Single(catalog.Subclasses, x => x.Name == "Troubadour");
        Assert.Equal(TraitType.Presence, troubadour.SpellcastTrait);
        Assert.Equal("Gifted Performer", troubadour.Foundation.Name);
        
        var bard = Assert.Single(catalog.Classes, x => x.Name == "Bard");
        Assert.Equal("Rally", bard.ClassFeature.Name);
        Assert.Equal(10, bard.BaseEvasion);
        Assert.Equal(DomainType.Grace, bard.Domain1);
        Assert.Equal(bard.Subclasses.Single(s => s.Name == "Troubadour"), troubadour);
        
        var clank =  Assert.Single(catalog.Ancestries, x => x.Name == "Clank");
        Assert.Contains("sentient mechanical beings", clank.Description);
        Assert.Equal("Efficient", clank.Features.First(x => x.Name == "Efficient").Name);
        
        var highborne = Assert.Single(catalog.Communities, x => x.Name == "Highborne");
        Assert.Contains("life of elegance, opulence, and prestige",  highborne.Description);
        Assert.Equal("Privilege",  highborne.Features.First(x => x.Name == "Privilege").Name);
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
    
    [Fact]
    public async Task LoadFileAsync_TrimsLoadedEntries()
    {
        var actual = await SrdJsonLoader.LoadFileAsync<TestInfo>(TestRepoPaths.LocalSrdJsonDirectory, "untrimmed.json", CancellationToken.None);

        Assert.Equal("Test McTestson", actual[0].Name);
        Assert.Equal("Streetway 1", actual[0].Address?.Street);
        Assert.Equal("1234",  actual[0].Address?.Code);
    }
}