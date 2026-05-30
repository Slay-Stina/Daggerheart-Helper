using Application.Dtos;
using Core.Entities;

namespace Application.Services;

public interface IArmorCatalogQueries
{
    List<Armor> GetAll();
    Armor GetById(Guid id);
    List<Armor> GetByTier(int tier);
    List<Armor> GetByFeatureId(Guid featureId);

    Task<List<ArmorSummary>> GetSummariesByTierAsync(int tier);
}
