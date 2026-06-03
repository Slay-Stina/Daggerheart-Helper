using Application.Dtos;
using Application.Services;
using Core.Entities;
using Core.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class CharacterService(IDbContextFactory<DaggerheartDbContext> factory) : ICharacterService
{
    public async Task<List<Character>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);
        return await context.Characters
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
    }

    public async Task<Character?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);
        return await context.Characters
            .AsNoTracking()
            .Include(c => c.GameClass)
            .Include(c => c.Subclass)
            .Include(c => c.Ancestry)
            .Include(c => c.Community)
            .Include(c => c.EquippedArmor)
            .Include(c => c.PrimaryWeapon)
            .Include(c => c.SecondaryWeapon)
            .Include(c => c.CharacterAbilities)
            .ThenInclude(ca => ca.Ability)
            .Include(c => c.Inventory)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task SaveAsync(Character character, CancellationToken cancellationToken = default)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);
        var existingItemIds = await LoadExistingInventoryItemIdsAsync(character.Inventory, context, cancellationToken);

        var isNew = character.Id == Guid.Empty;
        if (isNew)
        {
            character.Id = Guid.NewGuid();
            context.Characters.Add(character);
            foreach (var item in character.Inventory)
                if (item.Id != Guid.Empty
                    && existingItemIds.Contains(item.Id)
                    && context.Entry(item).State == EntityState.Added)
                    context.Entry(item).State = EntityState.Unchanged;
        }
        else
        {
            var existing = await context.Characters
                .Include(c => c.CharacterAbilities)
                .Include(c => c.Inventory)
                .FirstOrDefaultAsync(c => c.Id == character.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"Character '{character.Id}' was not found.");

            context.Entry(existing).CurrentValues.SetValues(character);

            existing.Traits = character.Traits;
            existing.DamageThresholds = character.DamageThresholds;
            existing.HitPoints = character.HitPoints;
            existing.Stress = character.Stress;
            existing.Hope = character.Hope;
            existing.ArmorSlots = character.ArmorSlots;

            existing.GameClassId = character.GameClassId;
            existing.SubclassId = character.SubclassId;
            existing.AncestryId = character.AncestryId;
            existing.CommunityId = character.CommunityId;
            existing.EquippedArmorId = character.EquippedArmorId;
            existing.PrimaryWeaponId = character.PrimaryWeaponId;
            existing.SecondaryWeaponId = character.SecondaryWeaponId;

            SyncCharacterAbilities(existing, character, context);
            SyncInventory(existing, character, context, existingItemIds);
        }

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
                a.Id, a.Title, a.DomainType, a.Level, a.RecallCost, a.Type, a.FeatureDescription))
            .ToListAsync(cancellationToken);
    }

    private static void SyncCharacterAbilities(Character existing, Character incoming, DaggerheartDbContext context)
    {
        context.CharacterAbilities.RemoveRange(existing.CharacterAbilities);

        foreach (var ca in incoming.CharacterAbilities)
        {
            if (ca.Ability != null)
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

        // Snapshot of all currently-tracked items before removal
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
                // Re-add a previously-removed item — reuse the tracked instance
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
