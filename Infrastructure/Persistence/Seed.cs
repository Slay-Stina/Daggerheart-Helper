using Core.Entities;
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
    await context.Database.EnsureCreatedAsync();
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
