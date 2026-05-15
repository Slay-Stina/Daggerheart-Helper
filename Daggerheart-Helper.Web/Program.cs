using Microsoft.FluentUI.AspNetCore.Components;
using Daggerheart_Helper.Web.Components;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Srd.Ingestion.Loading;
using Srd.Ingestion.Mapping;
using Core.Entities;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();
builder.Services.AddDaggerheartPersistence(connectionString);

var app = builder.Build();

// Seed SRD data on startup
await SeedSrdData(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseWebSockets();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Daggerheart_Helper.Shared._Imports).Assembly);

app.Run();

static async Task SeedSrdData(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<DaggerheartDbContext>();
    
    // Create database schema from model
    await context.Database.EnsureCreatedAsync();
    
    // Check if already seeded
    if (await context.GameClasses.AnyAsync())
        return;

    try
    {
        var loader = scope.ServiceProvider.GetRequiredService<ISrdJsonLoader>();
        
        // Navigate from bin/Debug/net9.0 up to the solution root
        // Path: Daggerheart-Helper.Web/bin/Debug/net9.0 -> need to go up 4 levels
        var basePath = AppContext.BaseDirectory;
        var projectRoot = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", ".."));
        var srdPath = Path.Combine(projectRoot, "External", "daggerheart-srd", ".build", "03_json");
        
        if (!Directory.Exists(srdPath))
        {
            Console.WriteLine($"⚠️  SRD JSON directory not found");
            Console.WriteLine("Skipping SRD data seed. Run with SRD files present to load data.");
            return;
        }

        Console.WriteLine($"📂 Loading SRD data from: {srdPath}");

        var catalog = await loader.LoadAsync(srdPath);
        
        // Build feature deduplication map
        var featureMap = new Dictionary<string, Feature>();
        Func<Feature?, Feature?> GetOrCreateFeature = (feature) =>
        {
            if (feature == null) return null;
            var key = $"{feature.Name}|{feature.Description}";
            if (!featureMap.ContainsKey(key))
            {
                featureMap[key] = feature;
            }
            return featureMap[key];
        };
        
        // Seed weapons
        try
        {
            Console.WriteLine($"💾 Seeding {catalog.Weapons.Count} weapons...");
            var weaponEntities = catalog.Weapons.ToEntities();
            
            // Deduplicate features for weapons
            foreach (var weapon in weaponEntities)
            {
                weapon.Feature = GetOrCreateFeature(weapon.Feature);
            }
            
            context.Weapons.AddRange(weaponEntities);
            await context.SaveChangesAsync();
            Console.WriteLine($"   ✓ {catalog.Weapons.Count} weapons seeded");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Failed to seed weapons: {ex.InnerException?.Message ?? ex.Message}");
        }
        
        // Seed armor
        try
        {
            Console.WriteLine($"💾 Seeding {catalog.Armors.Count} armor pieces...");
            var armorEntities = catalog.Armors.ToEntities();
            
            // Deduplicate features for armor
            foreach (var armor in armorEntities)
            {
                armor.Feature = GetOrCreateFeature(armor.Feature);
            }
            
            context.Armors.AddRange(armorEntities);
            await context.SaveChangesAsync();
            Console.WriteLine($"   ✓ {catalog.Armors.Count} armor seeded");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Failed to seed armor: {ex.InnerException?.Message ?? ex.Message}");
        }
        
        // Seed abilities
        try
        {
            Console.WriteLine($"💾 Seeding {catalog.Abilities.Count} abilities...");
            var abilityEntities = catalog.Abilities.ToEntities();
            context.Abilities.AddRange(abilityEntities);
            await context.SaveChangesAsync();
            Console.WriteLine($"   ✓ {catalog.Abilities.Count} abilities seeded");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Failed to seed abilities: {ex.InnerException?.Message ?? ex.Message}");
        }
        
        // Seed heritages (ancestries)
        try
        {
            Console.WriteLine($"💾 Seeding {catalog.Ancestries.Count} ancestries...");
            var heritageEntities = catalog.Ancestries.ToEntities();
            context.Heritages.AddRange(heritageEntities);
            await context.SaveChangesAsync();
            Console.WriteLine($"   ✓ {catalog.Ancestries.Count} ancestries seeded");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Failed to seed ancestries: {ex.InnerException?.Message ?? ex.Message}");
        }
        
        // Seed communities (heritage type: Community)
        try
        {
            Console.WriteLine($"💾 Seeding {catalog.Communities.Count} communities...");
            var communityEntities = catalog.Communities.ToEntities();

            // Deduplicate features for communities
            foreach (var com in communityEntities)
            {
                for (int i = 0; i < com.Features.Count; i++)
                {
                    com.Features[i] = GetOrCreateFeature(com.Features[i]);
                }
            }

            context.Heritages.AddRange(communityEntities);
            await context.SaveChangesAsync();
            Console.WriteLine($"   ✓ {catalog.Communities.Count} communities seeded");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Failed to seed communities: {ex.InnerException?.Message ?? ex.Message}");
        }
        
        // Seed classes (link to existing armor/weapons by name)
        try
        {
            Console.WriteLine($"💾 Seeding {catalog.Classes.Count} classes...");
            
            // Build lookup dictionaries from tracked entities
            var armorByName = context.Armors.ToDictionary(a => a.Name);
            var weaponsByName = context.Weapons.ToDictionary(w => w.Name);
            
            // Map classes with lookups
            var classEntities = catalog.Classes
                .Select(c => c.ToEntity(armorByName, weaponsByName))
                .ToList();
            
            context.GameClasses.AddRange(classEntities);
            await context.SaveChangesAsync();
            Console.WriteLine($"   ✓ {catalog.Classes.Count} classes seeded");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Failed to seed classes: {ex.InnerException?.Message ?? ex.Message}");
        }
        
        // Backfill feature backlinks (Hope features and Subclass features)
        try
        {
            Console.WriteLine("🔁 Backfilling feature backlinks for hope features and subclass features...");

            // Hope features: set Feature.GameClassIdAsHopeFeature = GameClass.Id where GameClass.HopeFeatureId references the Feature
            var classes = await context.GameClasses.ToListAsync();
            foreach (var gc in classes)
            {
                if (gc.HopeFeatureId.HasValue)
                {
                    var feat = await context.Features.FindAsync(gc.HopeFeatureId.Value);
                    if (feat != null)
                    {
                        feat.GameClassIdAsHopeFeature = gc.Id;
                    }
                }
            }

            // Subclass features: set Feature.SubclassId based on Subclass foundation/specialization/mastery ids
            var subclasses = await context.Subclasses.ToListAsync();
            foreach (var sc in subclasses)
            {
                if (sc.FoundationId != 0)
                {
                    var f = await context.Features.FindAsync(sc.FoundationId);
                    if (f != null) f.SubclassId = sc.Id;
                }
                if (sc.SpecializationId != 0)
                {
                    var f = await context.Features.FindAsync(sc.SpecializationId);
                    if (f != null) f.SubclassId = sc.Id;
                }
                if (sc.MasteryId != 0)
                {
                    var f = await context.Features.FindAsync(sc.MasteryId);
                    if (f != null) f.SubclassId = sc.Id;
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine("   ✓ Feature backlinks updated");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Failed to backfill feature backlinks: {ex.InnerException?.Message ?? ex.Message}");
        }
        
        Console.WriteLine($"✅ SRD data seeding complete!");
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException("Failed to seed SRD data. See inner exception for details.", ex);
    }
}
