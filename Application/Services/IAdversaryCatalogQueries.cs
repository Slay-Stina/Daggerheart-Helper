using Application.Dtos;
using Core.Enums;

namespace Application.Services;

public interface IAdversaryCatalogQueries
{
    Task<List<AdversarySummary>> GetAllAsync();
    Task<List<AdversarySummary>> SearchAsync(string? name, AdversaryType? type, int? tier, int? difficultyMin, int? difficultyMax);
    Task<AdversarySummary?> GetByIdAsync(Guid id);
}
