using Core.Entities;

namespace Application.Services;

public interface IAbilityCatalogQueries
{
    List<Ability> GetAll();
    Ability GetById(Guid id);
}
