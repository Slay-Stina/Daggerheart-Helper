using Application.Dtos;
using Application.Services;
using Core.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class ArmorCatalogQueries(DaggerheartDbContext context) : IArmorCatalogQueries
{
    private readonly DbSet<Armor> _armors = context.Armors;

    public List<Armor> GetAll() => _armors.AsNoTracking().Include(x => x.Feature).ToList();

    public List<Armor> GetByTier(int tier) => _armors.AsNoTracking().Include(x => x.Feature).Where(x => x.Tier == tier).ToList();

    public List<Armor> GetByFeatureId(Guid featureId) => _armors.AsNoTracking().Include(x => x.Feature).Where(x => x.FeatureId == featureId).ToList();

    public Armor GetById(Guid id) =>
        _armors.AsNoTracking().Include(x => x.Feature).FirstOrDefault(x => x.Id == id)
        ?? throw new KeyNotFoundException($"Armor '{id}' was not found.");

    public Task<List<ArmorSummary>> GetSummariesByTierAsync(int tier) =>
        _armors
            .AsNoTracking()
            .Where(a => a.Tier == tier)
            .Select(a => new ArmorSummary(
                a.Id,
                a.Name,
                a.Tier,
                a.ArmorScore,
                a.DamageThresholds,
                a.Feature != null ? new FeatureSummary(a.Feature.Id, a.Feature.Name, a.Feature.Description) : null))
            .ToListAsync();
}
