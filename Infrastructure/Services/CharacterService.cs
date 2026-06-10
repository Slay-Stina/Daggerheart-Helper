using Application.Dtos;
using Application.Services;
using Core.Entities;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class CharacterService(IDbContextFactory<DaggerheartDbContext> factory) : ICharacterService
{
    public async Task<List<CharacterSummary>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);
        var characters = await context.Characters
            .AsNoTracking()
            .Include(c => c.GameClass)
            .Include(c => c.Subclass)
            .Include(c => c.Ancestry)
            .Include(c => c.Community)
            .Include(c => c.EquippedArmor)
            .Include(c => c.PrimaryWeapon)
            .Include(c => c.SecondaryWeapon)
            .Include(c => c.Inventory)
            .ToListAsync(cancellationToken);
        return characters.Select(c => c.ToSummary()).ToList();
    }

    public async Task<CharacterSummary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);
        var character = await context.Characters
            .AsNoTracking()
            .Include(c => c.GameClass)
            .ThenInclude(c => c.HopeFeature)
            .Include(c => c.GameClass)
            .ThenInclude(c => c.ClassFeatures)
            .Include(c => c.Subclass)
            .ThenInclude(sc => sc.Foundation)
            .Include(c => c.Subclass)
            .ThenInclude(sc => sc.Specialization)
            .Include(c => c.Subclass)
            .ThenInclude(sc => sc.Mastery)
            .Include(c => c.Ancestry)
            .ThenInclude(h => h.Features)
            .Include(c => c.Community)
            .ThenInclude(h => h.Features)
            .Include(c => c.EquippedArmor)
            .ThenInclude(a => a.Feature)
            .Include(c => c.PrimaryWeapon)
            .ThenInclude(w => w.Feature)
            .Include(c => c.SecondaryWeapon)
            .ThenInclude(w => w.Feature)
            .Include(c => c.CharacterAbilities)
            .ThenInclude(ca => ca.Ability)
            .Include(c => c.Inventory)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        return character?.ToSummary();
    }

    public async Task SaveAsync(CharacterSummary summary, CancellationToken cancellationToken = default)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        var existingItemIds = await context.Items
            .Where(i => summary.Inventory.Select(x => x.Id).Contains(i.Id))
            .Select(i => i.Id)
            .ToHashSetAsync(cancellationToken);

        if (await context.Characters.AnyAsync(c => c.Id == summary.Id, cancellationToken))
        {
            await context.Database.ExecuteSqlRawAsync(
                "DELETE FROM CharacterAbilities WHERE CharacterId = @p0", [summary.Id], cancellationToken);
            await context.Database.ExecuteSqlRawAsync(
                "DELETE FROM CharacterItems WHERE CharacterId = @p0", [summary.Id], cancellationToken);
            await context.Database.ExecuteSqlRawAsync(
                "DELETE FROM Characters WHERE Id = @p0", [summary.Id], cancellationToken);
        }

        var entity = summary.ToNewCharacter();
        entity.Id = summary.Id == Guid.Empty ? entity.Id : summary.Id;
        foreach (var item in entity.Inventory)
            if (existingItemIds.Contains(item.Id))
                context.Entry(item).State = EntityState.Unchanged;
        context.Characters.Add(entity);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);
        var existing = await EnsureCharacterExistsAsync(id, cancellationToken, context);
        context.Characters.Remove(existing);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateCharacterAbilitiesAsync(Guid characterId, List<CharacterAbility> abilities,
        CancellationToken cancellationToken = default)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        var character = await context.Characters
            .Include(c => c.CharacterAbilities)
            .FirstOrDefaultAsync(c => c.Id == characterId, cancellationToken)
            ?? throw new KeyNotFoundException($"Character '{characterId}' was not found.");

        context.CharacterAbilities.RemoveRange(character.CharacterAbilities);

        foreach (var ca in abilities)
        {
            ca.CharacterId = characterId;
            if (ca.Ability != null)
                context.Entry(ca.Ability).State = EntityState.Unchanged;
            context.CharacterAbilities.Add(ca);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<AbilitySummary>> GetAvailableAbilitiesAsync(Guid characterId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        var character = await context.Characters
            .AsNoTracking()
            .Include(c => c.GameClass)
            .Include(c => c.CharacterAbilities)
            .FirstOrDefaultAsync(c => c.Id == characterId, cancellationToken)
            ?? throw new KeyNotFoundException($"Character '{characterId}' was not found.");

        var ownedAbilityIds = character.CharacterAbilities
            .Select(ca => ca.AbilityId)
            .ToHashSet();

        var domains = new[] { character.GameClass.Domain1, character.GameClass.Domain2 };

        return await context.Abilities
            .AsNoTracking()
            .Where(a => domains.Contains(a.DomainType)
                        && a.Level <= character.Level
                        && !ownedAbilityIds.Contains(a.Id))
            .OrderBy(a => a.Level)
            .ThenBy(a => a.DomainType)
            .ThenBy(a => a.Title)
            .Select(a => new AbilitySummary(
                a.Id, a.Title, a.DomainType, a.Level, a.RecallCost, a.Type, a.FeatureDescription, false))
            .ToListAsync(cancellationToken);
    }

    private static void SyncCharacterAbilities(Character existing, Character incoming, DaggerheartDbContext context)
    {
        context.CharacterAbilities.RemoveRange(existing.CharacterAbilities);

        foreach (var ca in incoming.CharacterAbilities)
        {
            context.Entry(ca.Ability).State = EntityState.Unchanged;

            existing.CharacterAbilities.Add(new CharacterAbility
            {
                CharacterId = existing.Id,
                AbilityId = ca.AbilityId,
                IsVaulted = ca.IsVaulted,
            });
        }
    }

    private static void SyncInventory(Character existing, Character incoming, DaggerheartDbContext context,
        HashSet<Guid> existingItemIds)
    {
        var incomingIds = incoming.Inventory.Select(i => i.Id).ToHashSet();

        // Snapshot of all currently tracked items before removal
        var allTrackedById = existing.Inventory.ToDictionary(i => i.Id);

        // Remove items no longer in the incoming list
        foreach (var item in existing.Inventory.ToList())
        {
            if (!incomingIds.Contains(item.Id))
                existing.Inventory.Remove(item);
        }

        var remainingIds = existing.Inventory.Select(i => i.Id).ToHashSet();

        foreach (var item in incoming.Inventory)
        {
            if (remainingIds.Contains(item.Id))
                continue;

            if (item.Id == Guid.Empty)
            {
                // New custom item
                existing.Inventory.Add(item);
            }
            else if (allTrackedById.TryGetValue(item.Id, out var tracked))
            {
                // Re-add a previously removed item — reuse the tracked instance
                existing.Inventory.Add(tracked);
            }
            else
            {
                // New attachment not previously tracked on this character
                if (item.Id != Guid.Empty && existingItemIds.Contains(item.Id))
                    context.Entry(item).State = EntityState.Unchanged;
                existing.Inventory.Add(item);
            }
        }
    }

    private static async Task<HashSet<Guid>> LoadExistingInventoryItemIdsAsync(List<Item> inventory,
        DaggerheartDbContext context, CancellationToken cancellationToken)
    {
        var incomingIds = inventory
            .Where(i => i.Id != Guid.Empty)
            .Select(i => i.Id)
            .Distinct()
            .ToList();

        if (incomingIds.Count == 0)
            return [];

        return await context.Items
            .AsNoTracking()
            .Where(i => incomingIds.Contains(i.Id))
            .Select(i => i.Id)
            .ToHashSetAsync(cancellationToken);
    }

    private static async Task<Character> EnsureCharacterExistsAsync(Guid characterId, CancellationToken cancellationToken,
        DaggerheartDbContext context)
    {
        var existing = await context.Characters.FindAsync([characterId], cancellationToken);
        return existing ?? throw new KeyNotFoundException($"Character '{characterId}' was not found.");
    }
}
