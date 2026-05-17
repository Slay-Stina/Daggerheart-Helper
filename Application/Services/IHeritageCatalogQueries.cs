using Core.Entities;

namespace Application.Services;

public interface IHeritageCatalogQueries
{
    List<Heritage> GetAll();
    Heritage GetById(Guid id);
    List<Heritage> GetAllAncestries();
    List<Heritage> GetAllCommunities();
}
