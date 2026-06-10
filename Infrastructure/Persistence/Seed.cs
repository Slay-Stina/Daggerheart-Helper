using Application.Dtos;
using Application.Services;
using Core.Entities;
using Core.Enums;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Srd.Ingestion.Loading;
using Srd.Ingestion.Mapping;

namespace Infrastructure.Persistence;

public class Seed
{
    public static async Task SrdData(IServiceProvider services, string srdPath)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<DaggerheartDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Seed>>();

        // Create database schema from model
        var created = await context.Database.EnsureCreatedAsync();
        if (!created)
        {
            // Database existed from prior deployment — check if schema is current
            var conn = context.Database.GetDbConnection();
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Adversaries'";
            var hasAdv = (long)(await cmd.ExecuteScalarAsync())! > 0;
            await conn.CloseAsync();
            if (!hasAdv)
            {
                logger.LogWarning("Database schema outdated (missing Adversaries). Recreating...");
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();
            }
        }
        await EnsureConcurrencyTriggersAsync(context);

        // Check if already seeded
        if (await context.GameClasses.AnyAsync())
            return;

        if (string.IsNullOrWhiteSpace(srdPath))
            throw new InvalidOperationException("SRD JSON path is not configured.");

        try
        {
            var loader = scope.ServiceProvider.GetRequiredService<ISrdJsonLoader>();

            if (!Directory.Exists(srdPath))
            {
                throw new DirectoryNotFoundException($"SRD JSON directory not found: {srdPath}");
            }

            logger.LogInformation("Loading SRD data from {SrdPath}", srdPath);

            var catalog = await loader.LoadAsync(srdPath);

            // Build feature deduplication map
            var featureMap = new Dictionary<string, Feature>();

            Feature? GetOrCreateFeature(Feature? feature)
            {
                if (feature == null) return null;
                var key = $"{feature.Name}|{feature.Description}";
                featureMap.TryAdd(key, feature);
                return featureMap[key];
            }

            async Task SaveChangesForBatchAsync(string batchName)
            {
                try
                {
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to seed {batchName}.", ex);
                }
            }

            logger.LogInformation("Seeding {WeaponCount} weapons...", catalog.Weapons.Count);
            var weaponEntities = catalog.Weapons.ToEntities();

            // Deduplicate features for weapons
            foreach (var weapon in weaponEntities)
            {
                weapon.Feature = GetOrCreateFeature(weapon.Feature);
            }

            context.Weapons.AddRange(weaponEntities);
            await SaveChangesForBatchAsync("weapons");
            logger.LogInformation("{WeaponCount} weapons seeded", catalog.Weapons.Count);

            logger.LogInformation("Seeding {ArmorCount} armor pieces...", catalog.Armors.Count);
            var armorEntities = catalog.Armors.ToEntities();

            // Deduplicate features for armor
            foreach (var armor in armorEntities)
            {
                armor.Feature = GetOrCreateFeature(armor.Feature);
            }

            context.Armors.AddRange(armorEntities);
            await SaveChangesForBatchAsync("armor");
            logger.LogInformation("{ArmorCount} armor pieces seeded", catalog.Armors.Count);

            logger.LogInformation("Seeding {AbilityCount} abilities...", catalog.Abilities.Count);
            var abilityEntities = catalog.Abilities.ToEntities();
            context.Abilities.AddRange(abilityEntities);
            await SaveChangesForBatchAsync("abilities");
            logger.LogInformation("{AbilityCount} abilities seeded", catalog.Abilities.Count);

            logger.LogInformation("Seeding {ItemCount} items...", catalog.Items.Count);
            var itemEntities = catalog.Items.ToEntities();
            context.Items.AddRange(itemEntities);
            await SaveChangesForBatchAsync("items");
            logger.LogInformation("{ItemCount} SRD items seeded", catalog.Items.Count);

            // Seed base items not in SRD (torch, rope, etc.)
            logger.LogInformation("Seeding base items...");
            if (!await context.Items.AnyAsync(i => i.Name == "Torch"))
            {
                context.Items.AddRange(
                    new Item { Name = "Torch", Description = "A simple torch that provides light." },
                    new Item { Name = "50 ft rope", Description = "A coil of sturdy rope." },
                    new Item { Name = "Basic supplies", Description = "General adventuring supplies." },
                    new Item { Name = "Minor Health Potion", Description = "Restores some health." },
                    new Item { Name = "Minor Stamina Potion", Description = "Restores some stamina." });
                await SaveChangesForBatchAsync("base items");
            }
            logger.LogInformation("Base items seeded");

            logger.LogInformation("Seeding {AncestryCount} ancestries...", catalog.Ancestries.Count);
            var heritageEntities = catalog.Ancestries.ToEntities();
            context.Heritages.AddRange(heritageEntities);
            await SaveChangesForBatchAsync("ancestries");
            logger.LogInformation("{AncestryCount} ancestries seeded", catalog.Ancestries.Count);

            logger.LogInformation("Seeding {CommunityCount} communities...", catalog.Communities.Count);
            var communityEntities = catalog.Communities.ToEntities();

            // Deduplicate features for communities
            foreach (var com in communityEntities)
            {
                for (int i = 0; i < com.Features.Count; i++)
                {
                    com.Features[i] = GetOrCreateFeature(com.Features[i]) ?? throw new InvalidOperationException();
                }
            }

            context.Heritages.AddRange(communityEntities);
            await SaveChangesForBatchAsync("communities");
            logger.LogInformation("{CommunityCount} communities seeded", catalog.Communities.Count);

            logger.LogInformation("Seeding {AdversaryCount} adversaries...", catalog.Adversaries.Count);
            var adversaryEntities = catalog.Adversaries.ToEntities();
            context.Adversaries.AddRange(adversaryEntities);
            await SaveChangesForBatchAsync("adversaries");
            logger.LogInformation("{AdversaryCount} adversaries seeded", catalog.Adversaries.Count);

            logger.LogInformation("Seeding {ClassCount} classes...", catalog.Classes.Count);

            // Build lookup dictionaries from tracked entities
            var armorByName = context.Armors.ToDictionary(a => a.Name);
            var weaponsByName = context.Weapons.ToDictionary(w => w.Name);

            // Map classes with lookups
            var classEntities = catalog.Classes
                .Select(c => c.ToEntity(armorByName, weaponsByName))
                .ToList();

            context.GameClasses.AddRange(classEntities);
            await SaveChangesForBatchAsync("classes");
            logger.LogInformation("{ClassCount} classes seeded", catalog.Classes.Count);

            logger.LogInformation("SRD data seeding complete.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to seed SRD data.");
            throw new InvalidOperationException("Failed to seed SRD data. See inner exception for details.", ex);
        }
    }

    public static async Task SampleCharacter(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ICharacterService>();
        var context = scope.ServiceProvider.GetRequiredService<DaggerheartDbContext>();

        if (await service.GetAllAsync() is { Count: > 0 })
            return;

        var gameClass = await context.GameClasses.FirstAsync(g => g.Name == "Guardian");
        var subclass = await context.Subclasses.FirstAsync(s => s.Name == "Stalwart");
        var ancestry = await context.Heritages.FirstAsync(h => h.Name == "Dwarf" && h.HeritageType == HeritageType.Ancestry);
        var community = await context.Heritages.FirstAsync(h => h.Name == "Ridgeborne" && h.HeritageType == HeritageType.Community);
        var armor = await context.Armors.FirstAsync(a => a.Name == "Chainmail Armor");
        var weapon = await context.Weapons.FirstAsync(w => w.Name == "Longsword" && w.Tier == 1);

        var itemNames = new[] { "Torch", "50 ft rope", "Basic supplies", "Minor Health Potion" };
        var catalogItems = await context.Items
            .Where(i => itemNames.Contains(i.Name))
            .ToListAsync();
        var inventoryItems = itemNames
            .Select(name => catalogItems.FirstOrDefault(i => i.Name == name) ?? new Item { Name = name, Description = "" })
            .ToList();

        var feat = (Guid id, string name) => new FeatureSummary(id, name, "");
        var ability1 = await context.Abilities.FirstAsync(a => a.Title == "I Am Your Shield");
        var ability2 = await context.Abilities.FirstAsync(a => a.Title == "Not Good Enough");

        var summary = new CharacterSummary(
            Guid.NewGuid(), 1, "Borin Ironhide", "he/him", null, null, null, null, null,
            new ClassCardSummary(gameClass.Id, gameClass.Name, gameClass.Description, gameClass.Domain1, gameClass.Domain2,
                gameClass.BaseEvasion, gameClass.BaseHealth, [], feat(gameClass.HopeFeatureId, gameClass.HopeFeature.Name)),
            new SubclassSummary(subclass.Id, subclass.Name, subclass.Description, feat(subclass.Foundation.Id, subclass.Foundation.Name),
                feat(subclass.Specialization.Id, subclass.Specialization.Name), feat(subclass.Mastery.Id, subclass.Mastery.Name)),
            null, null,
            new HeritageSummary(ancestry.Id, ancestry.Name, ancestry.Description, ancestry.HeritageType, []),
            new HeritageSummary(community.Id, community.Name, community.Description, community.HeritageType, []),
            new TraitScores(1, 2, -1, 0, 0, 1),
            new DamageThresholds(5, 3), 10, 0,
            new[] { "Survived the Goblin Wars", "Guardian of the Mountain Pass" },
            new Dictionary<string, string>
            {
                ["Where were you born?"] = "The Ironforge Mountains",
                ["Why did you become an adventurer?"] = "To protect my homeland from the darkness",
                ["What was your childhood like?"] = "Harsh but honorable, trained in the Forgehold garrison"
            },
            inventoryItems.Cast<ItemSummary>().ToList(), 1, null,
            new ArmorSummary(armor.Id, armor.Name, armor.Tier, armor.ArmorScore, armor.DamageThresholds,
                armor.Feature is not null ? new FeatureSummary(armor.Feature.Id, armor.Feature.Name, armor.Feature.Description) : null),
            new WeaponSummary(weapon.Id, weapon.Name, weapon.Tier, weapon.Damage, weapon.Burden, weapon.RangeType, weapon.Trait, weapon.Category),
            null,
            new[] { new AbilitySummary(ability1.Id, ability1.Title, ability1.DomainType, ability1.Level, ability1.RecallCost, ability1.Type, ability1.FeatureDescription, false),
                    new AbilitySummary(ability2.Id, ability2.Title, ability2.DomainType, ability2.Level, ability2.RecallCost, ability2.Type, ability2.FeatureDescription, false) },
            new ResourcePool(7, 7), new ResourcePool(0, 6), new ResourcePool(2, 6), new ResourcePool(4, 4),
            Array.Empty<byte>());

        await service.SaveAsync(summary);
    }

    public static async Task EnsureConcurrencyTriggersAsync(DaggerheartDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync(
            "CREATE TRIGGER IF NOT EXISTS UpdateCharacterRowVersion " +
            "AFTER UPDATE ON Characters " +
            "BEGIN " +
            "    UPDATE Characters SET RowVersion = randomblob(8) WHERE rowid = NEW.rowid; " +
            "END;");
    }

}
