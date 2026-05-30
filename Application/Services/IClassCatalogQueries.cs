using Application.Dtos;
using Core.Entities;

namespace Application.Services;

public interface IClassCatalogQueries
{
    List<GameClass> GetAll();
    GameClass GetById(Guid id);

    Task<List<ClassCardSummary>> GetCardSummariesAsync();
    Task<List<SubclassSummary>> GetSubclassesAsync(Guid classId);
    Task<ClassSetupData> GetClassSetupDataAsync(Guid classId);
}
