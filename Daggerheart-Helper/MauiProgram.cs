using Android.Content.Res;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace Daggerheart_Helper;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        var dbDir = FileSystem.AppDataDirectory;
        var dbPath = Path.Combine(dbDir, "daggerheart.db");
        var connectionString = $"Data Source={dbPath}";

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddFluentUIComponents();
        builder.Services.AddDaggerheartPersistence(connectionString);
        builder.Services.AddCatalogServices();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        SeedDatabase(app.Services);

        return app;
    }

    private static void SeedDatabase(IServiceProvider services)
    {
        Android.Util.Log.Debug("DHELPER", "SeedDatabase started");

        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DaggerheartDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Seed>>();

        Android.Util.Log.Debug("DHELPER", "EnsureCreated...");
        context.Database.EnsureCreated();
        Android.Util.Log.Debug("DHELPER", "EnsureCreated done");

        Seed.EnsureConcurrencyTriggersAsync(context).GetAwaiter().GetResult();
        Android.Util.Log.Debug("DHELPER", "Concurrency triggers created");

        if (context.GameClasses.Any())
        {
            logger.LogInformation("Database already seeded.");
            return;
        }

        ThreadPool.QueueUserWorkItem(_ => DoSeed(services, logger));
    }

    private static void DoSeed(IServiceProvider services, ILogger<Seed> logger)
    {
        try
        {
            Android.Util.Log.Debug("DHELPER", "Starting SRD extraction...");
            var srdDir = ExtractSrdAssets(logger);
            if (srdDir is null)
            {
                Android.Util.Log.Warn("DHELPER", "ExtractSrdAssets returned null");
                return;
            }
            Android.Util.Log.Debug("DHELPER", $"SRD extracted to {srdDir}");

            logger.LogInformation("Seeding database from {SrdDir}...", srdDir);
            using var scope = services.CreateScope();
            Seed.SrdData(scope.ServiceProvider, srdDir).GetAwaiter().GetResult();
            logger.LogInformation("Database seeded successfully.");

            Directory.Delete(srdDir, true);
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("DHELPER", $"Seed failed: {ex}");
            logger.LogWarning(ex, "Failed to seed SRD data. App will run with empty catalog.");
        }
    }

    private static string? ExtractSrdAssets(ILogger<Seed> logger)
    {
        var tempDir = Path.Combine(FileSystem.CacheDirectory, "srd");
        if (Directory.Exists(tempDir))
            Directory.Delete(tempDir, true);
        Directory.CreateDirectory(tempDir);

        var assetFiles = new[] {
            "armor.json", "weapons.json", "abilities.json",
            "ancestries.json", "communities.json",
            "subclasses.json", "classes.json"
        };

        try
        {
            var assetManager = Android.App.Application.Context?.Assets;
            if (assetManager is null)
            {
                Android.Util.Log.Warn("DHELPER", "Android AssetManager not available.");
                logger.LogWarning("Android AssetManager not available.");
                return null;
            }
            using var assets = assetManager;
            foreach (var file in assetFiles)
            {
                var assetPath = $"srd/{file}";
                var destPath = Path.Combine(tempDir, file);
                Android.Util.Log.Debug("DHELPER", $"Extracting {assetPath} -> {destPath}");

                using var stream = assets.Open(assetPath) ?? throw new FileNotFoundException($"Asset not found: {assetPath}");
                using var fileStream = File.Create(destPath);
                stream.CopyTo(fileStream);
                Android.Util.Log.Debug("DHELPER", $"Extracted {file}");
            }
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("DHELPER", $"Failed to extract SRD assets: {ex}");
            logger.LogWarning(ex, "Failed to extract SRD assets. Path: srd/");
            try { Directory.Delete(tempDir, true); } catch { }
            return null;
        }

        return tempDir;
    }
}
