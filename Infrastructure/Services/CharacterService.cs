using Application.Services;
using Core.Entities;
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
            .Include(c => c.DomainAbilities)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task SaveAsync(Character character, CancellationToken cancellationToken = default)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        // If the character has navigational properties populated, attach their tracked IDs to prevent EF from recreating them.
        // We ensure we only set the foreign key IDs.
        var isNew = character.Id == Guid.Empty;
        if (isNew)
        {
            character.Id = Guid.NewGuid();
            
            // Clean navigation properties before saving to prevent EF from trying to insert existing entities (like GameClass, Heritage, etc.)
            var entry = context.Entry(character);
            
            // Attach existing related entities to context to inform EF they already exist
            if (character.GameClass != null) context.Entry(character.GameClass).State = EntityState.Unchanged;
            if (character.Subclass != null) context.Entry(character.Subclass).State = EntityState.Unchanged;
            if (character.Ancestry != null) context.Entry(character.Ancestry).State = EntityState.Unchanged;
            if (character.Community != null) context.Entry(character.Community).State = EntityState.Unchanged;
            if (character.EquippedArmor != null) context.Entry(character.EquippedArmor).State = EntityState.Unchanged;
            if (character.PrimaryWeapon != null) context.Entry(character.PrimaryWeapon).State = EntityState.Unchanged;
            if (character.SecondaryWeapon != null) context.Entry(character.SecondaryWeapon).State = EntityState.Unchanged;
            foreach (var ab in character.DomainAbilities)
                context.Entry(ab).State = EntityState.Unchanged;

            context.Characters.Add(character);
        }
        else
        {
            var existing = await context.Characters.FindAsync([character.Id], cancellationToken);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Character '{character.Id}' was not found.");
            }

            // Update simple properties
            context.Entry(existing).CurrentValues.SetValues(character);

            // Update complex owned properties
            existing.Traits = character.Traits;
            existing.DamageThresholds = character.DamageThresholds;
            existing.HitPoints = character.HitPoints;
            existing.Stress = character.Stress;
            existing.Hope = character.Hope;
            existing.ArmorSlots = character.ArmorSlots;

            // Update many-to-many DomainAbilities (only if provided)
            if (character.DomainAbilities.Count > 0)
            {
                existing.DomainAbilities.Clear();
                foreach (var ab in character.DomainAbilities)
                {
                    context.Entry(ab).State = EntityState.Unchanged;
                    existing.DomainAbilities.Add(ab);
                }
            }

            // Update foreign keys
            existing.GameClassId = character.GameClassId;
            existing.SubclassId = character.SubclassId;
            existing.AncestryId = character.AncestryId;
            existing.CommunityId = character.CommunityId;
            existing.EquippedArmorId = character.EquippedArmorId;
            existing.PrimaryWeaponId = character.PrimaryWeaponId;
            existing.SecondaryWeaponId = character.SecondaryWeaponId;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);
        var existing = await context.Characters.FindAsync([id], cancellationToken);
        if (existing != null)
        {
            context.Characters.Remove(existing);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
