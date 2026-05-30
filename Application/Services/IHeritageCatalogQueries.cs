using Application.Dtos;
using Core.Entities;
using Core.Enums;

namespace Application.Services;

public interface IHeritageCatalogQueries
{
    List<Heritage> GetAll();
    Heritage GetById(Guid id);
    List<Heritage> GetAllAncestries();
    List<Heritage> GetAllCommunities();

    Task<List<HeritageSummary>> GetSummariesAsync(HeritageType type);
}
