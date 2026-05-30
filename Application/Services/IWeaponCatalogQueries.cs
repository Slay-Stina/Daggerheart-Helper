using Application.Dtos;
using Core.Entities;
using Core.Enums;

namespace Application.Services;

public interface IWeaponCatalogQueries
{
    List<Weapon> GetAll();
    Weapon GetById(Guid id);
    List<Weapon> GetByFeatureId(Guid featureId);
    List<Weapon> GetByTier(int tier);
    List<Weapon> GetByTraitType(TraitType trait);

    Task<List<WeaponSummary>> GetSummariesByTierAsync(int tier);
}
