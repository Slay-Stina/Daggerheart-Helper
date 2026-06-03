using Core.Entities;

namespace Application.Services;

public interface IItemCatalogQueries
{
    Task<List<Item>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
