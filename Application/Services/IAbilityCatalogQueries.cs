using Application.Dtos;
using Core.Entities;
using Core.Enums;

namespace Application.Services;

public interface IAbilityCatalogQueries
{
    List<Ability> GetAll();
    Ability GetById(Guid id);

    Task<List<AbilitySummary>> GetByDomainAsync(DomainType domain, int level);
}
