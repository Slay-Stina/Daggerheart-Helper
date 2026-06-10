using Application.Dtos;
using Application.Services;
using Core.Entities;
using Core.Enums;
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
        return characters.Select(MapToSummary).ToList();
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
            .Include(c => c.Ancestry)
            .Include(c => c.Community)
            .Include(c => c.EquippedArmor)
            .Include(c => c.PrimaryWeapon)
            .Include(c => c.SecondaryWeapon)
            .Include(c => c.CharacterAbilities)
            .ThenInclude(ca => ca.Ability)
            .Include(c => c.Inventory)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        return character is null ? null : MapToSummary(character);
    }

    public async Task SaveAsync(CharacterSummary summary, CancellationToken cancellationToken = default)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        var isNew = summary.Id == Guid.Empty;
        if (isNew)
        {
            var character = new Character
            {
                Id = Guid.NewGuid(),
                Name = summary.Name,
                Pronouns = summary.Pronouns,
                DescriptionEyes = summary.DescriptionEyes,
                DescriptionBody = summary.DescriptionBody,
                DescriptionClothes = summary.DescriptionClothes,
                DescriptionSkin = summary.DescriptionSkin,
                DescriptionAttitude = summary.DescriptionAttitude,
                Level = summary.Level,
                GameClassId = summary.Class.Id,
                SubclassId = summary.Subclass.Id,
                AncestryId = summary.Ancestry.Id,
                CommunityId = summary.Community.Id,
                EquippedArmorId = summary.EquippedArmor?.Id,
                PrimaryWeaponId = summary.PrimaryWeapon?.Id,
                SecondaryWeaponId = summary.SecondaryWeapon?.Id,
                Traits = summary.Traits,
                DamageThresholds = summary.DamageThresholds,
                Evasion = summary.Evasion,
                Experiences = summary.Experiences.ToList(),
                BackgroundAnswers = summary.BackgroundAnswers.SelectMany(kvp => new[] { kvp.Key, kvp.Value }).ToList(),
                GoldHandfuls = summary.GoldHandfuls,
                SpellFocus = summary.SpellFocus,
                HitPoints = summary.HitPoints,
                Stress = summary.Stress,
                Hope = summary.Hope,
                ArmorSlots = summary.ArmorSlots,
                Inventory = summary.Inventory.Select(i => new Item { Id = i.Id, Name = i.Name, Description = "" }).ToList(),
                CharacterAbilities = summary.CharacterAbilities.Select(a => new CharacterAbility
                {
                    AbilityId = a.Id,
                    IsVaulted = false,
                }).ToList(),
            };
            context.Characters.Add(character);
        }
        else
        {
            var existing = await context.Characters
                .Include(c => c.CharacterAbilities)
                .Include(c => c.Inventory)
                .FirstOrDefaultAsync(c => c.Id == summary.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"Character '{summary.Id}' was not found.");

            existing.Name = summary.Name;
            existing.Pronouns = summary.Pronouns;
            existing.DescriptionEyes = summary.DescriptionEyes;
            existing.DescriptionBody = summary.DescriptionBody;
            existing.DescriptionClothes = summary.DescriptionClothes;
            existing.DescriptionSkin = summary.DescriptionSkin;
            existing.DescriptionAttitude = summary.DescriptionAttitude;
            existing.Traits = summary.Traits;
            existing.DamageThresholds = summary.DamageThresholds;
            existing.HitPoints = summary.HitPoints;
            existing.Stress = summary.Stress;
            existing.Hope = summary.Hope;
            existing.ArmorSlots = summary.ArmorSlots;
            existing.EquippedArmorId = summary.EquippedArmor?.Id;
            existing.PrimaryWeaponId = summary.PrimaryWeapon?.Id;
            existing.SecondaryWeaponId = summary.SecondaryWeapon?.Id;
            existing.Experiences = summary.Experiences.ToList();
            existing.GoldHandfuls = summary.GoldHandfuls;
            existing.SpellFocus = summary.SpellFocus;
            existing.BackgroundAnswers = summary.BackgroundAnswers.SelectMany(kvp => new[] { kvp.Key, kvp.Value }).ToList();
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
                a.Id, a.Title, a.DomainType, a.Level, a.RecallCost, a.Type, a.FeatureDescription, false))
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

    private static CharacterSummary MapToSummary(Character c)
    {
        return new CharacterSummary(
            c.Id, c.Level, c.Name, c.Pronouns,
            c.DescriptionEyes, c.DescriptionBody, c.DescriptionClothes, c.DescriptionSkin, c.DescriptionAttitude,
            new ClassCardSummary(c.GameClass.Id, c.GameClass.Name, c.GameClass.Description, c.GameClass.Domain1, c.GameClass.Domain2,
                c.GameClass.BaseEvasion, c.GameClass.BaseHealth,
                c.GameClass.ClassFeatures.Select(f => new FeatureSummary(f.Id, f.Name, f.Description)).ToList(),
                new FeatureSummary(c.GameClass.HopeFeature.Id, c.GameClass.HopeFeature.Name, c.GameClass.HopeFeature.Description)),
            new SubclassSummary(c.Subclass.Id, c.Subclass.Name, c.Subclass.Description,
                new FeatureSummary(c.Subclass.Foundation.Id, c.Subclass.Foundation.Name, c.Subclass.Foundation.Description),
                new FeatureSummary(c.Subclass.Specialization.Id, c.Subclass.Specialization.Name, c.Subclass.Specialization.Description),
                new FeatureSummary(c.Subclass.Mastery.Id, c.Subclass.Mastery.Name, c.Subclass.Mastery.Description)),
            null, null,
            new HeritageSummary(c.Ancestry.Id, c.Ancestry.Name, c.Ancestry.Description, c.Ancestry.HeritageType,
                c.Ancestry.Features.Select(f => new FeatureSummary(f.Id, f.Name, f.Description)).ToList()),
            new HeritageSummary(c.Community.Id, c.Community.Name, c.Community.Description, c.Community.HeritageType,
                c.Community.Features.Select(f => new FeatureSummary(f.Id, f.Name, f.Description)).ToList()),
            c.Traits, c.DamageThresholds, c.Evasion, c.Proficiency,
            c.Experiences,
            BuildBackgroundDictionary(c.BackgroundAnswers),
            c.Inventory.Select(i => new ItemSummary(i.Id, i.Name, i.Description)).ToList(),
            c.GoldHandfuls, c.SpellFocus,
            c.EquippedArmor is not null
                ? new ArmorSummary(c.EquippedArmor.Id, c.EquippedArmor.Name, c.EquippedArmor.Tier, c.EquippedArmor.ArmorScore, c.EquippedArmor.DamageThresholds,
                    c.EquippedArmor.Feature is not null ? new FeatureSummary(c.EquippedArmor.Feature.Id, c.EquippedArmor.Feature.Name, c.EquippedArmor.Feature.Description) : null)
                : null,
            c.PrimaryWeapon is not null
                ? new WeaponSummary(c.PrimaryWeapon.Id, c.PrimaryWeapon.Name, c.PrimaryWeapon.Tier, c.PrimaryWeapon.Damage, c.PrimaryWeapon.Burden, c.PrimaryWeapon.RangeType, c.PrimaryWeapon.Trait, c.PrimaryWeapon.Category,
                    c.PrimaryWeapon.Feature is not null ? new FeatureSummary(c.PrimaryWeapon.Feature.Id, c.PrimaryWeapon.Feature.Name, c.PrimaryWeapon.Feature.Description) : null)
                : null,
            c.SecondaryWeapon is not null
                ? new WeaponSummary(c.SecondaryWeapon.Id, c.SecondaryWeapon.Name, c.SecondaryWeapon.Tier, c.SecondaryWeapon.Damage, c.SecondaryWeapon.Burden, c.SecondaryWeapon.RangeType, c.SecondaryWeapon.Trait, c.SecondaryWeapon.Category,
                    c.SecondaryWeapon.Feature is not null ? new FeatureSummary(c.SecondaryWeapon.Feature.Id, c.SecondaryWeapon.Feature.Name, c.SecondaryWeapon.Feature.Description) : null)
                : null,
            c.CharacterAbilities.Select(ca => new AbilitySummary(ca.AbilityId, ca.Ability!.Title, ca.Ability.DomainType, ca.Ability.Level, ca.Ability.RecallCost, ca.Ability.Type, ca.Ability.FeatureDescription, ca.IsVaulted)),
            c.HitPoints, c.Stress, c.Hope, c.ArmorSlots, c.RowVersion);
    }

    private static Dictionary<string, string> BuildBackgroundDictionary(List<string> answers)
    {
        var dict = new Dictionary<string, string>();
        for (int i = 0; i < answers.Count - 1; i += 2)
            dict[answers[i]] = answers[i + 1];
        return dict;
    }

    private static async Task<Character> EnsureCharacterExistsAsync(Guid characterId, CancellationToken cancellationToken,
        DaggerheartDbContext context)
    {
        var existing = await context.Characters.FindAsync([characterId], cancellationToken);
        return existing ?? throw new KeyNotFoundException($"Character '{characterId}' was not found.");
    }
}
