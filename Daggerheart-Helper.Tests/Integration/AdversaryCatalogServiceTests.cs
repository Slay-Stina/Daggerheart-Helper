using Application.Services;
using Core.Enums;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DaggerheartHelper.Tests.Integration;

public class AdversaryCatalogServiceTests : IDisposable
{
    private readonly DaggerheartDbContext _context;
    private readonly IAdversaryCatalogQueries _service;

    public AdversaryCatalogServiceTests()
    {
        var options = new DbContextOptionsBuilder<DaggerheartDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new DaggerheartDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        SeedTestData();

        _service = new AdversaryCatalogQueries(_context);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllAdversaries()
    {
        var result = await _service.GetAllAsync();

        Assert.Equal(4, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_OrdersByTierThenName()
    {
        var result = await _service.GetAllAsync();

        Assert.Equal("Tier1B", result[0].Name);
        Assert.Equal("Tier1Z", result[1].Name);
        Assert.Equal("Burrower", result[2].Name);
        Assert.Equal("Tier3B", result[3].Name);
    }

    [Fact]
    public async Task SearchAsync_FilterByName()
    {
        var result = await _service.SearchAsync("Burrow", null, null, null, null);

        Assert.Single(result);
        Assert.Equal("Burrower", result[0].Name);
    }

    [Fact]
    public async Task SearchAsync_FilterByName_IsCaseInsensitive()
    {
        var result = await _service.SearchAsync("burrower", null, null, null, null);

        Assert.Single(result);
    }

    [Fact]
    public async Task SearchAsync_FilterByName_PartialMatch()
    {
        var result = await _service.SearchAsync("Tier", null, null, null, null);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task SearchAsync_FilterByName_EmptyStringReturnsAll()
    {
        var result = await _service.SearchAsync("", null, null, null, null);

        Assert.Equal(4, result.Count);
    }

    [Fact]
    public async Task SearchAsync_FilterByType()
    {
        var result = await _service.SearchAsync(null, AdversaryType.Solo, null, null, null);

        Assert.Single(result);
        Assert.Equal("Burrower", result[0].Name);
    }

    [Fact]
    public async Task SearchAsync_FilterByTier()
    {
        var result = await _service.SearchAsync(null, null, 1, null, null);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task SearchAsync_FilterByDifficultyMin()
    {
        var result = await _service.SearchAsync(null, null, null, 13, null);

        Assert.Single(result);
        Assert.Equal("Tier3B", result[0].Name);
    }

    [Fact]
    public async Task SearchAsync_FilterByDifficultyMax()
    {
        var result = await _service.SearchAsync(null, null, null, null, 12);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task SearchAsync_CombinedFilters()
    {
        var result = await _service.SearchAsync("Tier3", AdversaryType.Bruiser, 3, 14, 15);

        Assert.Single(result);
        Assert.Equal("Tier3B", result[0].Name);
    }

    [Fact]
    public async Task SearchAsync_NoMatchReturnsEmpty()
    {
        var result = await _service.SearchAsync("nonexistent", null, null, null, null);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectAdversary()
    {
        var all = await _service.GetAllAsync();
        var target = all.First(a => a.Name == "Burrower");

        var result = await _service.GetByIdAsync(target.Id);

        Assert.NotNull(result);
        Assert.Equal("Burrower", result.Name);
        Assert.Equal(AdversaryType.Solo, result.Type);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_IncludesFeatures()
    {
        var result = await _service.GetAllAsync();
        var burrower = result.First(a => a.Name == "Burrower");

        Assert.Equal(2, burrower.Features.Count);
        Assert.Contains(burrower.Features, f => f.Name == "Burrow");
    }

    private void SeedTestData()
    {
        var adversaries = new List<Core.Entities.Adversary>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Burrower",
                Description = "A digging beast.",
                Tier = 2, Type = AdversaryType.Solo, Difficulty = 12,
                Hp = 10, Stress = 4, Thresholds = "6/12",
                Atk = "+3", Attack = "Claws", Damage = "1d8+2 phy",
                Range = "Melee", Experience = "Burrow +2",
                MotivesAndTactics = "Dig, ambush",
                Features = new List<Core.Entities.Feature>
                {
                    new() { Id = Guid.NewGuid(), Name = "Burrow", Description = "Can burrow underground." },
                    new() { Id = Guid.NewGuid(), Name = "Ambush", Description = "Surprise attack." }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Tier3B",
                Description = "A bruiser.",
                Tier = 3, Type = AdversaryType.Bruiser, Difficulty = 14,
                Hp = 20, Stress = 5, Thresholds = "8/16",
                Atk = "+5", Attack = "Fists", Damage = "2d6+3 phy",
                Range = "Melee", Experience = "",
                MotivesAndTactics = "Pummel",
                Features = new List<Core.Entities.Feature>()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Tier1Z",
                Description = "A minion.",
                Tier = 1, Type = AdversaryType.Minion, Difficulty = 10,
                Hp = 4, Stress = 0, Thresholds = "4/8",
                Atk = "+1", Attack = "Dagger", Damage = "1d4 phy",
                Range = "Melee", Experience = "",
                MotivesAndTactics = "Swarm",
                Features = new List<Core.Entities.Feature>()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Tier1B",
                Description = "A skulk.",
                Tier = 1, Type = AdversaryType.Skulk, Difficulty = 11,
                Hp = 6, Stress = 1, Thresholds = "5/10",
                Atk = "+2", Attack = "Shortbow", Damage = "1d6+1 phy",
                Range = "Far", Experience = "",
                MotivesAndTactics = "Snipe, hide",
                Features = new List<Core.Entities.Feature>()
            }
        };

        _context.Adversaries.AddRange(adversaries);
        _context.SaveChanges();
    }
}
