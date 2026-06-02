using Application.Services;
using Core.Entities;
using Core.Enums;
using Core.ValueObjects;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DaggerheartHelper.Tests.Integration;

public class CharacterCreationTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private IDbContextFactory<DaggerheartDbContext> _factory = null!;
    private ICharacterService _service = null!;
    private ServiceProvider _provider = null!;

    private Guid _gameClassId;
    private Guid _subclassId;
    private Guid _ancestryId;
    private Guid _communityId;
    private Guid _abilityId1;
    private Guid _abilityId2;
    private Guid _itemId1;
    private Guid _itemId2;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddDbContextFactory<DaggerheartDbContext>(options =>
            options.UseSqlite(_connection));
        services.AddScoped<ICharacterService, CharacterService>();

        _provider = services.BuildServiceProvider();
        _factory = _provider.GetRequiredService<IDbContextFactory<DaggerheartDbContext>>();
        _service = _provider.GetRequiredService<ICharacterService>();

        await SeedTestDataAsync();
    }

    private async Task SeedTestDataAsync()
    {
        await using var context = await _factory.CreateDbContextAsync();
        await context.Database.EnsureCreatedAsync();
        await Seed.EnsureConcurrencyTriggersAsync(context);

        _gameClassId = Guid.NewGuid();
        _subclassId = Guid.NewGuid();
        _ancestryId = Guid.NewGuid();
        _communityId = Guid.NewGuid();
        _abilityId1 = Guid.NewGuid();
        _abilityId2 = Guid.NewGuid();
        _itemId1 = Guid.NewGuid();
        _itemId2 = Guid.NewGuid();

        var gameClass = new GameClass
        {
            Id = _gameClassId,
            Name = "Test Class",
            Description = "A test class",
            BaseEvasion = 10,
            BaseHealth = 5,
            Domain1 = DomainType.Arcana,
            Domain2 = DomainType.Blade,
            SuggestedTraits = new TraitScores(0, 0, 0, 0, 0, 0),
            BackgroundQuestions = [],
            ConnectionQuestions = [],
            Items = [],
        };

        var foundationFeature = new Feature
        {
            Id = Guid.NewGuid(),
            Name = "Foundation",
            Description = "Foundation feature",
        };

        var specializationFeature = new Feature
        {
            Id = Guid.NewGuid(),
            Name = "Specialization",
            Description = "Specialization feature",
        };

        var masteryFeature = new Feature
        {
            Id = Guid.NewGuid(),
            Name = "Mastery",
            Description = "Mastery feature",
        };

        var subclass = new Subclass
        {
            Id = _subclassId,
            Name = "Test Subclass",
            Description = "A test subclass",
            GameClass = gameClass,
            Foundation = foundationFeature,
            Specialization = specializationFeature,
            Mastery = masteryFeature,
        };

        var ancestry = new Heritage
        {
            Id = _ancestryId,
            Name = "Test Ancestry",
            Description = "Test ancestry",
            HeritageType = HeritageType.Ancestry,
        };

        var community = new Heritage
        {
            Id = _communityId,
            Name = "Test Community",
            Description = "Test community",
            HeritageType = HeritageType.Community,
        };

        var ability1 = new Ability
        {
            Id = _abilityId1,
            Title = "Fireball",
            DomainType = DomainType.Arcana,
            Level = 1,
            RecallCost = 0,
            Type = AbilityType.Spell,
            FeatureDescription = "A fireball spell",
        };

        var ability2 = new Ability
        {
            Id = _abilityId2,
            Title = "Sword Strike",
            DomainType = DomainType.Blade,
            Level = 1,
            RecallCost = 0,
            Type = AbilityType.Ability,
            FeatureDescription = "A sword strike",
        };

        context.GameClasses.Add(gameClass);
        await context.SaveChangesAsync();

        context.Subclasses.Add(subclass);
        await context.SaveChangesAsync();

        context.Heritages.AddRange(ancestry, community);
        await context.SaveChangesAsync();

        context.Abilities.AddRange(ability1, ability2);
        await context.SaveChangesAsync();

        var item1 = new Item
        {
            Id = _itemId1,
            Name = "Torch",
            Description = "A torch",
        };
        var item2 = new Item
        {
            Id = _itemId2,
            Name = "Rope",
            Description = "A coil of rope",
        };
        context.Items.AddRange(item1, item2);
        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _connection.CloseAsync();
        await _connection.DisposeAsync();
        await _provider.DisposeAsync();
    }

    [Fact]
    public async Task CreateCharacter_WithAllFields_SavesAndLoads()
    {
        var character = new Character
        {
            Name = "Test Hero",
            Level = 1,
            GameClassId = _gameClassId,
            SubclassId = _subclassId,
            AncestryId = _ancestryId,
            CommunityId = _communityId,
            Traits = new TraitScores(1, 2, 0, 0, 0, 0),
            DamageThresholds = new DamageThresholds(2, 4),
            Evasion = 10,
            HitPoints = new ResourcePool(5, 5),
            Stress = new ResourcePool(0, 5),
            Hope = new ResourcePool(2, 5),
            ArmorSlots = new ResourcePool(0, 0),
            Experiences = ["First Adventure", "Brave Deed"],
            BackgroundAnswers = ["What is your goal?", "To explore", "Who inspires you?", "My mentor"],
            Inventory =
            [
                new Item { Id = _itemId1, Name = "Torch" },
                new Item { Id = _itemId2, Name = "Rope" },
            ],
            GoldHandfuls = 3,
            CharacterAbilities =
            [
                new CharacterAbility { AbilityId = _abilityId1, IsVaulted = false },
                new CharacterAbility { AbilityId = _abilityId2, IsVaulted = true },
            ],
        };

        await _service.SaveAsync(character);

        Assert.NotEqual(Guid.Empty, character.Id);

        var loaded = await _service.GetByIdAsync(character.Id);
        Assert.NotNull(loaded);
        Assert.Equal("Test Hero", loaded.Name);
        Assert.Equal(1, loaded.Level);
        Assert.Equal(3, loaded.GoldHandfuls);
        Assert.Equal(2, loaded.Experiences.Count);
        Assert.Equal(1, loaded.Traits.Agility);
        Assert.Equal(2, loaded.Traits.Strength);
        Assert.Equal(10, loaded.Evasion);
        Assert.Equal(5, loaded.HitPoints.Max);
        Assert.Equal(5, loaded.HitPoints.Current);

        Assert.NotNull(loaded.CharacterAbilities);
        Assert.Equal(2, loaded.CharacterAbilities.Count);
        Assert.Single(loaded.CharacterAbilities, ca => !ca.IsVaulted);
        Assert.Single(loaded.CharacterAbilities, ca => ca.IsVaulted);

        Assert.Equal(2, loaded.Inventory.Count);
        Assert.Contains(loaded.Inventory, i => i.Name == "Torch");
        Assert.Contains(loaded.Inventory, i => i.Name == "Rope");
    }

    [Fact]
    public async Task CreateCharacter_ThenEditAndSave_UpdatesCorrectly()
    {
        var character = new Character
        {
            Name = "Level 1 Hero",
            Level = 1,
            GameClassId = _gameClassId,
            SubclassId = _subclassId,
            AncestryId = _ancestryId,
            CommunityId = _communityId,
            Traits = new TraitScores(0, 0, 0, 0, 0, 0),
            DamageThresholds = new DamageThresholds(0, 0),
            Evasion = 10,
            HitPoints = new ResourcePool(5, 5),
            Stress = new ResourcePool(0, 5),
            Hope = new ResourcePool(2, 5),
            ArmorSlots = new ResourcePool(0, 0),
            CharacterAbilities =
            [
                new CharacterAbility { AbilityId = _abilityId1, IsVaulted = false },
            ],
        };

        await _service.SaveAsync(character);

        character.Name = "Updated Hero";
        character.Level = 2;
        character.HitPoints = new ResourcePool(3, 7);
        character.Experiences = ["First Quest", "Dragon Slayer"];
        character.GoldHandfuls = 10;
        character.CharacterAbilities =
        [
            new CharacterAbility { AbilityId = _abilityId1, IsVaulted = true },
            new CharacterAbility { AbilityId = _abilityId2, IsVaulted = false },
        ];

        await _service.SaveAsync(character);

        var loaded = await _service.GetByIdAsync(character.Id);
        Assert.NotNull(loaded);
        Assert.Equal("Updated Hero", loaded.Name);
        Assert.Equal(2, loaded.Level);
        Assert.Equal(3, loaded.HitPoints.Current);
        Assert.Equal(7, loaded.HitPoints.Max);
        Assert.Equal(10, loaded.GoldHandfuls);
        Assert.Equal(2, loaded.Experiences.Count);
        Assert.Contains("Dragon Slayer", loaded.Experiences);

        Assert.Equal(2, loaded.CharacterAbilities.Count);
        Assert.Single(loaded.CharacterAbilities, ca => ca.AbilityId == _abilityId1 && ca.IsVaulted);
        Assert.Single(loaded.CharacterAbilities, ca => ca.AbilityId == _abilityId2 && !ca.IsVaulted);
    }

    [Fact]
    public async Task GetAvailableAbilitiesAsync_ReturnsCorrectFilteredList()
    {
        await using (var context = await _factory.CreateDbContextAsync())
        {
            context.Abilities.Add(new Ability
            {
                Id = Guid.NewGuid(),
                Title = "Advanced Fire",
                DomainType = DomainType.Arcana,
                Level = 3,
                RecallCost = 1,
                Type = AbilityType.Spell,
                FeatureDescription = "A level 3 spell",
            });
            await context.SaveChangesAsync();
        }

        var character = new Character
        {
            Name = "Wizard",
            Level = 2,
            GameClassId = _gameClassId,
            SubclassId = _subclassId,
            AncestryId = _ancestryId,
            CommunityId = _communityId,
            Traits = new TraitScores(0, 0, 0, 0, 0, 0),
            DamageThresholds = new DamageThresholds(0, 0),
            Evasion = 10,
            HitPoints = new ResourcePool(5, 5),
            Stress = new ResourcePool(0, 5),
            Hope = new ResourcePool(2, 5),
            ArmorSlots = new ResourcePool(0, 0),
            CharacterAbilities =
            [
                new CharacterAbility { AbilityId = _abilityId1, IsVaulted = false },
            ],
        };

        await _service.SaveAsync(character);

        var available = await _service.GetAvailableAbilitiesAsync(character.Id);

        Assert.Contains(available, a => a.Id == _abilityId2);
        Assert.DoesNotContain(available, a => a.Id == _abilityId1);
        Assert.All(available, a => Assert.True(a.Level <= 2));
        Assert.All(available, a => Assert.Contains(a.DomainType, new[] { DomainType.Arcana, DomainType.Blade }));
    }

    [Fact]
    public async Task CreateCharacter_WithMinimumFields_SavesSuccessfully()
    {
        var character = new Character
        {
            Name = "Minimal Hero",
            GameClassId = _gameClassId,
            SubclassId = _subclassId,
            AncestryId = _ancestryId,
            CommunityId = _communityId,
            Traits = new TraitScores(0, 0, 0, 0, 0, 0),
            DamageThresholds = new DamageThresholds(0, 0),
            Evasion = 10,
            HitPoints = new ResourcePool(5, 5),
            Stress = new ResourcePool(0, 5),
            Hope = new ResourcePool(2, 5),
            ArmorSlots = new ResourcePool(0, 0),
        };

        await _service.SaveAsync(character);

        var loaded = await _service.GetByIdAsync(character.Id);
        Assert.NotNull(loaded);
        Assert.Equal("Minimal Hero", loaded.Name);
        Assert.Equal(1, loaded.Level);
        Assert.Empty(loaded.Experiences);
        Assert.Empty(loaded.Inventory);
        Assert.Empty(loaded.CharacterAbilities);
    }

    [Fact]
    public async Task EditCharacter_AddAndRemoveInventory_SyncsCorrectly()
    {
        var character = new Character
        {
            Name = "Inventory Hero",
            Level = 1,
            GameClassId = _gameClassId,
            SubclassId = _subclassId,
            AncestryId = _ancestryId,
            CommunityId = _communityId,
            Traits = new TraitScores(0, 0, 0, 0, 0, 0),
            DamageThresholds = new DamageThresholds(0, 0),
            Evasion = 10,
            HitPoints = new ResourcePool(5, 5),
            Stress = new ResourcePool(0, 5),
            Hope = new ResourcePool(2, 5),
            ArmorSlots = new ResourcePool(0, 0),
            Inventory =
            [
                new Item { Id = _itemId1, Name = "Torch" },
            ],
        };

        await _service.SaveAsync(character);

        // Edit: replace Torch with Rope (different item instance — simulates UI rebuild)
        character.Inventory =
        [
            new Item { Id = _itemId2, Name = "Rope" },
        ];
        await _service.SaveAsync(character);

        var loaded = await _service.GetByIdAsync(character.Id);
        Assert.NotNull(loaded);
        Assert.Single(loaded.Inventory);
        Assert.Equal("Rope", loaded.Inventory[0].Name);

        // Edit: add Torch back (previously tracked → now detached → re-attach)
        character.Inventory =
        [
            new Item { Id = _itemId2, Name = "Rope" },
            new Item { Id = _itemId1, Name = "Torch" },
        ];
        await _service.SaveAsync(character);

        loaded = await _service.GetByIdAsync(character.Id);
        Assert.NotNull(loaded);
        Assert.Equal(2, loaded.Inventory.Count);
        Assert.Contains(loaded.Inventory, i => i.Name == "Torch");
        Assert.Contains(loaded.Inventory, i => i.Name == "Rope");
    }
}
