using Application.Services;
using Core.Entities;
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

        var itemNames = new[] { "Torch", "50 ft rope", "Basic supplies", "Minor Health Potion" };
        var catalogItems = await context.Items
            .Where(i => itemNames.Contains(i.Name))
            .ToListAsync();
        var inventoryItems = itemNames
            .Select(name => catalogItems.FirstOrDefault(i => i.Name == name) ?? new Item { Name = name, Description = "" })
            .ToList();

        var character = new Character
        {
            Name = "Borin Ironhide",
            Pronouns = "he/him",
            Level = 1,
            GameClassId = Guid.Parse("D17B68ED-FBBA-4F49-8C8D-5D372871593A"),
            SubclassId = Guid.Parse("8847BD64-62C5-45C9-9FE3-EAEBA6CF7DD1"),
            AncestryId = Guid.Parse("C3C7D875-B717-437C-88CD-77D049452FFB"),
            CommunityId = Guid.Parse("1A996A6E-1381-4384-B370-436FEA5EF74E"),
            EquippedArmorId = Guid.Parse("748526B9-AF83-4C61-B864-91376AF6791A"),
            PrimaryWeaponId = Guid.Parse("DB5D5C9C-2B28-4C36-A536-C4DCEB59B747"),
            Traits = new TraitScores(Agility: 1, Strength: 2, Finesse: -1, Instinct: 0, Presence: 0, Knowledge: 1),
            DamageThresholds = new DamageThresholds(Major: 5, Severe: 3),
            Evasion = 10,
            HitPoints = new ResourcePool(7, 7),
            Stress = new ResourcePool(0, 6),
            Hope = new ResourcePool(2, 6),
            ArmorSlots = new ResourcePool(4, 4),
            GoldHandfuls = 1,
            Experiences = new List<string> { "Survived the Goblin Wars", "Guardian of the Mountain Pass" },
            BackgroundAnswers = new List<string>
            {
                "Where were you born?", "The Ironforge Mountains",
                "Why did you become an adventurer?", "To protect my homeland from the darkness",
                "What was your childhood like?", "Harsh but honorable, trained in the Forgehold garrison"
            },
            Inventory = inventoryItems,
            CharacterAbilities = new List<CharacterAbility>
            {
                new() { AbilityId = Guid.Parse("C017284D-B04D-4C0B-90EA-95C31C869F63"), IsVaulted = false },
                new() { AbilityId = Guid.Parse("BA159592-DFF9-4A48-8D3A-C2BB25A7B12B"), IsVaulted = false },
            },
        };

        await service.SaveAsync(character);
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
