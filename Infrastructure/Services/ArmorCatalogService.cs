using Application.Services;
using Core.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class ArmorCatalogQueries(DaggerheartDbContext context) : IArmorCatalogQueries
{
    private readonly DbSet<Armor> _armors = context.Armors;

    public List<Armor> GetAll() => _armors.AsNoTracking().ToList();

    public List<Armor> GetByTier(int tier) => _armors.AsNoTracking().Where(x => x.Tier == tier).ToList();

    public List<Armor> GetByFeatureId(Guid featureId) => _armors.AsNoTracking().Where(x => x.FeatureId == featureId).ToList();

    public Armor GetById(Guid id) =>
        _armors.AsNoTracking().FirstOrDefault(x => x.Id == id)
        ?? throw new KeyNotFoundException($"Armor '{id}' was not found.");
}
