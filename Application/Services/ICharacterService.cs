using Application.Dtos;
using Core.Entities;

namespace Application.Services;

public interface ICharacterService
{
    Task<List<Character>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Character?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveAsync(Character character, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateCharacterAbilitiesAsync(Guid characterId, List<CharacterAbility> abilities,
        CancellationToken cancellationToken = default);
    Task<List<AbilitySummary>> GetAvailableAbilitiesAsync(Guid characterId,
        CancellationToken cancellationToken = default);
}
