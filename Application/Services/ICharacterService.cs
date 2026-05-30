using Core.Entities;

namespace Application.Services;

public interface ICharacterService
{
    Task<List<Character>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Character?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveAsync(Character character, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
