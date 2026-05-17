using Core.Entities;

namespace Application.Services;

public interface IClassCatalogQueries
{
    List<GameClass> GetAll();
    GameClass GetById(Guid id);
}
