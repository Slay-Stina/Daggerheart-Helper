using Application.Services;
using Core.Entities;
using Core.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class WeaponCatalogQueries(DaggerheartDbContext context) : IWeaponCatalogQueries
{
    private readonly DbSet<Weapon> _weapons = context.Weapons;

    public List<Weapon> GetAll() => _weapons.AsNoTracking().ToList();

    public List<Weapon> GetByFeatureId(Guid featureId) => _weapons.AsNoTracking().Where(x => x.FeatureId == featureId).ToList();

    public List<Weapon> GetByTier(int tier) => _weapons.AsNoTracking().Where(x => x.Tier == tier).ToList();

    public List<Weapon> GetByTraitType(TraitType trait) => _weapons.AsNoTracking().Where(x => x.Trait == trait).ToList();

    public Weapon GetById(Guid id) =>
        _weapons.AsNoTracking().FirstOrDefault(x => x.Id == id)
        ?? throw new KeyNotFoundException($"Weapon '{id}' was not found.");
}
