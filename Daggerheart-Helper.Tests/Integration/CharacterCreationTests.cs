using Application.Dtos;
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
    private Guid _hopeFeatureId;
    private Guid _foundationId;
    private Guid _specializationId;
    private Guid _masteryId;

    private CharacterSummary MakeCharacter(Guid? id = null, string name = "Test Hero", int level = 1,
        int gold = 1, IEnumerable<string>? experiences = null, Dictionary<string, string>? background = null,
        IEnumerable<ItemSummary>? inventory = null, IEnumerable<AbilitySummary>? abilities = null,
        ResourcePool? hp = null)
    {
        return new CharacterSummary(
            id ?? Guid.NewGuid(), level, name, null, null, null, null, null, null,
            new ClassCardSummary(_gameClassId, "Test Class", "A test class", DomainType.Arcana, DomainType.Blade, 10, 5, [],
                new FeatureSummary(_hopeFeatureId, "Hope Feature", "Hope feature")),
            new SubclassSummary(_subclassId, "Test Subclass", "A test subclass",
                new FeatureSummary(_foundationId, "Foundation", "Foundation feature"),
                new FeatureSummary(_specializationId, "Specialization", "Specialization feature"),
                new FeatureSummary(_masteryId, "Mastery", "Mastery feature")),
            null, null,
            new HeritageSummary(_ancestryId, "Test Ancestry", "Test ancestry", HeritageType.Ancestry, []),
            new HeritageSummary(_communityId, "Test Community", "Test community", HeritageType.Community, []),
            new TraitScores(0, 0, 0, 0, 0, 0), new DamageThresholds(0, 0), 10, 0,
            experiences ?? [], background ?? new Dictionary<string, string>(),
            inventory ?? [], gold, null, null, null, null,
            abilities ?? [],
            hp ?? new ResourcePool(5, 5), new ResourcePool(0, 5), new ResourcePool(2, 5), new ResourcePool(0, 0),
            Array.Empty<byte>());
    }

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

        _hopeFeatureId = Guid.NewGuid();
        var hopeFeature = new Feature
        {
            Id = _hopeFeatureId,
            Name = "Hope Feature",
            Description = "Hope feature",
        };

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
            HopeFeatureId = hopeFeature.Id,
            HopeFeature = hopeFeature,
        };

        _foundationId = Guid.NewGuid();
        var foundationFeature = new Feature
        {
            Id = _foundationId,
            Name = "Foundation",
            Description = "Foundation feature",
        };

        _specializationId = Guid.NewGuid();
        var specializationFeature = new Feature
        {
            Id = _specializationId,
            Name = "Specialization",
            Description = "Specialization feature",
        };

        _masteryId = Guid.NewGuid();
        var masteryFeature = new Feature
        {
            Id = _masteryId,
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
        var summary = new CharacterSummary(
            Guid.NewGuid(), 1, "Test Hero", null, null, null, null, null, null,
            new ClassCardSummary(_gameClassId, "Test Class", "A test class", DomainType.Arcana, DomainType.Blade, 10, 5, [],
                new FeatureSummary(_hopeFeatureId, "Hope Feature", "Hope feature")),
            new SubclassSummary(_subclassId, "Test Subclass", "A test subclass",
                new FeatureSummary(_foundationId, "Foundation", "Foundation feature"),
                new FeatureSummary(_specializationId, "Specialization", "Specialization feature"),
                new FeatureSummary(_masteryId, "Mastery", "Mastery feature")),
            null, null,
            new HeritageSummary(_ancestryId, "Test Ancestry", "Test ancestry", HeritageType.Ancestry, []),
            new HeritageSummary(_communityId, "Test Community", "Test community", HeritageType.Community, []),
            new TraitScores(1, 2, 0, 0, 0, 0),
            new DamageThresholds(2, 4), 10, 0,
            new[] { "First Adventure", "Brave Deed" },
            new Dictionary<string, string> { ["What is your goal?"] = "To explore", ["Who inspires you?"] = "My mentor" },
            new[] { new ItemSummary(_itemId1, "Torch", "A torch"), new ItemSummary(_itemId2, "Rope", "A coil of rope") },
            3, null, null, null, null,
            new[] {
                new AbilitySummary(_abilityId1, "Fireball", DomainType.Arcana, 1, 0, AbilityType.Spell, "A fireball spell", false),
                new AbilitySummary(_abilityId2, "Sword Strike", DomainType.Blade, 1, 0, AbilityType.Ability, "A sword strike", true),
            },
            new ResourcePool(5, 5), new ResourcePool(0, 5), new ResourcePool(2, 5), new ResourcePool(0, 0),
            Array.Empty<byte>());

        await _service.SaveAsync(summary);

        var loaded = await _service.GetByIdAsync(summary.Id);
        Assert.NotNull(loaded);
        Assert.Equal("Test Hero", loaded.Name);
        Assert.Equal(1, loaded.Level);
        Assert.Equal(3, loaded.GoldHandfuls);
        Assert.Equal(2, loaded.Experiences.Count());
        Assert.Equal(1, loaded.Traits.Agility);
        Assert.Equal(2, loaded.Traits.Strength);
        Assert.Equal(10, loaded.Evasion);
        Assert.Equal(5, loaded.HitPoints.Max);
        Assert.Equal(5, loaded.HitPoints.Current);

        Assert.NotNull(loaded.CharacterAbilities);
        Assert.Equal(2, loaded.CharacterAbilities.Count());
        Assert.Single(loaded.CharacterAbilities, ca => !ca.IsVaulted);
        Assert.Single(loaded.CharacterAbilities, ca => ca.IsVaulted);

        Assert.Equal(2, loaded.Inventory.Count());
        Assert.Contains(loaded.Inventory, i => i.Name == "Torch");
        Assert.Contains(loaded.Inventory, i => i.Name == "Rope");
    }

    [Fact]
    public async Task CreateCharacter_ThenEditAndSave_UpdatesCorrectly()
    {
        var heroId = Guid.NewGuid();
        var hero = new CharacterSummary(
            heroId, 1, "Level 1 Hero", null, null, null, null, null, null,
            new ClassCardSummary(_gameClassId, "Test Class", "A test class", DomainType.Arcana, DomainType.Blade, 10, 5, [],
                new FeatureSummary(_hopeFeatureId, "Hope Feature", "Hope feature")),
            new SubclassSummary(_subclassId, "Test Subclass", "A test subclass",
                new FeatureSummary(_foundationId, "Foundation", "Foundation feature"),
                new FeatureSummary(_specializationId, "Specialization", "Specialization feature"),
                new FeatureSummary(_masteryId, "Mastery", "Mastery feature")),
            null, null,
            new HeritageSummary(_ancestryId, "Test Ancestry", "Test ancestry", HeritageType.Ancestry, []),
            new HeritageSummary(_communityId, "Test Community", "Test community", HeritageType.Community, []),
            new TraitScores(0, 0, 0, 0, 0, 0),
            new DamageThresholds(0, 0), 10, 0, [], new Dictionary<string, string>(), [],
            1, null, null, null, null,
            new[] { new AbilitySummary(_abilityId1, "Fireball", DomainType.Arcana, 1, 0, AbilityType.Spell, "A fireball spell", false) },
            new ResourcePool(5, 5), new ResourcePool(0, 5), new ResourcePool(2, 5), new ResourcePool(0, 0),
            Array.Empty<byte>());

        await _service.SaveAsync(hero);

        var updated = new CharacterSummary(
            heroId, 2, "Updated Hero", null, null, null, null, null, null,
            new ClassCardSummary(_gameClassId, "Test Class", "A test class", DomainType.Arcana, DomainType.Blade, 10, 5, [],
                new FeatureSummary(_hopeFeatureId, "Hope Feature", "Hope feature")),
            new SubclassSummary(_subclassId, "Test Subclass", "A test subclass",
                new FeatureSummary(_foundationId, "Foundation", "Foundation feature"),
                new FeatureSummary(_specializationId, "Specialization", "Specialization feature"),
                new FeatureSummary(_masteryId, "Mastery", "Mastery feature")),
            null, null,
            new HeritageSummary(_ancestryId, "Test Ancestry", "Test ancestry", HeritageType.Ancestry, []),
            new HeritageSummary(_communityId, "Test Community", "Test community", HeritageType.Community, []),
            new TraitScores(0, 0, 0, 0, 0, 0),
            new DamageThresholds(0, 0), 10, 0,
            new[] { "First Quest", "Dragon Slayer" },
            new Dictionary<string, string>(), [],
            10, null, null, null, null,
            new[] {
                new AbilitySummary(_abilityId1, "Fireball", DomainType.Arcana, 1, 0, AbilityType.Spell, "A fireball spell", true),
                new AbilitySummary(_abilityId2, "Sword Strike", DomainType.Blade, 1, 0, AbilityType.Ability, "A sword strike", false),
            },
            new ResourcePool(3, 7), new ResourcePool(0, 5), new ResourcePool(2, 5), new ResourcePool(0, 0),
            Array.Empty<byte>());

        await _service.SaveAsync(updated);

        var loaded = await _service.GetByIdAsync(heroId);
        Assert.NotNull(loaded);
        Assert.Equal("Updated Hero", loaded.Name);
        Assert.Equal(2, loaded.Level);
        Assert.Equal(3, loaded.HitPoints.Current);
        Assert.Equal(7, loaded.HitPoints.Max);
        Assert.Equal(10, loaded.GoldHandfuls);
        Assert.Equal(2, loaded.Experiences.Count());
        Assert.Contains("Dragon Slayer", loaded.Experiences);

        Assert.Equal(2, loaded.CharacterAbilities.Count());
        Assert.Single(loaded.CharacterAbilities, ca => ca.Id == _abilityId1 && ca.IsVaulted);
        Assert.Single(loaded.CharacterAbilities, ca => ca.Id == _abilityId2 && !ca.IsVaulted);
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

        var wizardId = Guid.NewGuid();
        var wizard = new CharacterSummary(
            wizardId, 2, "Wizard", null, null, null, null, null, null,
            new ClassCardSummary(_gameClassId, "Test Class", "A test class", DomainType.Arcana, DomainType.Blade, 10, 5, [],
                new FeatureSummary(_hopeFeatureId, "Hope Feature", "Hope feature")),
            new SubclassSummary(_subclassId, "Test Subclass", "A test subclass",
                new FeatureSummary(_foundationId, "Foundation", "Foundation feature"),
                new FeatureSummary(_specializationId, "Specialization", "Specialization feature"),
                new FeatureSummary(_masteryId, "Mastery", "Mastery feature")),
            null, null,
            new HeritageSummary(_ancestryId, "Test Ancestry", "Test ancestry", HeritageType.Ancestry, []),
            new HeritageSummary(_communityId, "Test Community", "Test community", HeritageType.Community, []),
            new TraitScores(0, 0, 0, 0, 0, 0),
            new DamageThresholds(0, 0), 10, 0, [], new Dictionary<string, string>(), [],
            1, null, null, null, null,
            new[] { new AbilitySummary(_abilityId1, "Fireball", DomainType.Arcana, 1, 0, AbilityType.Spell, "A fireball spell", false) },
            new ResourcePool(5, 5), new ResourcePool(0, 5), new ResourcePool(2, 5), new ResourcePool(0, 0),
            Array.Empty<byte>());

        await _service.SaveAsync(wizard);

        var available = await _service.GetAvailableAbilitiesAsync(wizardId);

        Assert.Contains(available, a => a.Id == _abilityId2);
        Assert.DoesNotContain(available, a => a.Id == _abilityId1);
        Assert.All(available, a => Assert.True(a.Level <= 2));
        Assert.All(available, a => Assert.Contains(a.DomainType, new[] { DomainType.Arcana, DomainType.Blade }));
    }

    [Fact]
    public async Task CreateCharacter_WithMinimumFields_SavesSuccessfully()
    {
        var minimalId = Guid.NewGuid();
        await _service.SaveAsync(MakeCharacter(minimalId, "Minimal Hero"));

        var loaded = await _service.GetByIdAsync(minimalId);
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
        var invId = Guid.NewGuid();
        var r1 = new ItemSummary(_itemId1, "Torch", "");
        var r2 = new ItemSummary(_itemId2, "Rope", "");
        await _service.SaveAsync(MakeCharacter(invId, "Inventory Hero", inventory: [r1]));

        // Edit: replace Torch with Rope
        await _service.SaveAsync(MakeCharacter(invId, "Inventory Hero", inventory: [r2]));

        var loaded = await _service.GetByIdAsync(invId);
        Assert.NotNull(loaded);
        Assert.Single(loaded.Inventory);
        Assert.Equal("Rope", loaded.Inventory.First().Name);

        // Edit: add Torch back
        await _service.SaveAsync(MakeCharacter(invId, "Inventory Hero", inventory: [r2, r1]));

        loaded = await _service.GetByIdAsync(invId);
        Assert.NotNull(loaded);
        Assert.Equal(2, loaded.Inventory.Count());
        Assert.Contains(loaded.Inventory, i => i.Name == "Torch");
        Assert.Contains(loaded.Inventory, i => i.Name == "Rope");
    }

    [Fact]
    public async Task SaveAsync_WithCustomInventoryItem_AssignsAndPersistsItem()
    {
        var customItemId = Guid.NewGuid();
        var customInv = new[] { new ItemSummary(customItemId, "Homebrew Bomb", "Custom explosive") };
        await _service.SaveAsync(MakeCharacter(Guid.NewGuid(), "Custom Item Hero", inventory: customInv));

        await using var context = await _factory.CreateDbContextAsync();
        Assert.NotNull(await context.Items.FindAsync(customItemId));
    }

    [Fact]
    public async Task UpdateCharacterAbilitiesAsync_WithAbilityIdOnly_UpdatesWithoutThrowing()
    {
        var abId = Guid.NewGuid();
        var abs = new[] { new AbilitySummary(_abilityId1, "", DomainType.Arcana, 1, 0, AbilityType.Spell, "", false) };
        await _service.SaveAsync(MakeCharacter(abId, "Ability Hero", abilities: abs));

        await _service.UpdateCharacterAbilitiesAsync(abId,
        [
            new CharacterAbility { AbilityId = _abilityId1, IsVaulted = false },
            new CharacterAbility { AbilityId = _abilityId2, IsVaulted = true },
        ]);

        var loaded = await _service.GetByIdAsync(abId);
        Assert.NotNull(loaded);
        Assert.Equal(2, loaded.CharacterAbilities.Count());
        Assert.Contains(loaded.CharacterAbilities, ca => ca.Id == _abilityId1 && !ca.IsVaulted);
        Assert.Contains(loaded.CharacterAbilities, ca => ca.Id == _abilityId2 && ca.IsVaulted);
    }
}
