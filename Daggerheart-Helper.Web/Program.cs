using Microsoft.FluentUI.AspNetCore.Components;
using Daggerheart_Helper.Web.Components;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Srd.Ingestion.Loading;
using Srd.Ingestion.Mapping;

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
    
    // Check if already seeded
    if (await context.GameClasses.AnyAsync())
        return;

    try
    {
        var loader = scope.ServiceProvider.GetRequiredService<ISrdJsonLoader>();
        var srdPath = Path.Combine(AppContext.BaseDirectory, "External", "daggerheart-srd", ".build", "03_json");
        
        if (!Directory.Exists(srdPath))
            throw new DirectoryNotFoundException($"SRD JSON directory not found: {srdPath}");

        var catalog = await loader.LoadAsync(srdPath);

        // Add entities to database
        context.Armors.AddRange(catalog.Armors.Select(a => a.ToEntity()));
        context.Weapons.AddRange(catalog.Weapons.Select(w => w.ToEntity()));
        context.Abilities.AddRange(catalog.Abilities.Select(a => a.ToEntity()));
        context.Heritages.AddRange(catalog.Ancestries.Select(h => h.ToEntity()));
        context.Heritages.AddRange(catalog.Communities.Select(h => h.ToEntity()));
        context.GameClasses.AddRange(catalog.Classes.Select(c => c.ToEntity()));

        await context.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException("Failed to seed SRD data. See inner exception for details.", ex);
    }
}
