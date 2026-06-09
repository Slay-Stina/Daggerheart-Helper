using Microsoft.FluentUI.AspNetCore.Components;
using Daggerheart_Helper.Web.Components;
using Infrastructure;
using Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var srdJsonPath = builder.Configuration["Srd:JsonPath"];

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
}

if (string.IsNullOrWhiteSpace(srdJsonPath))
{
    throw new InvalidOperationException("Configuration value 'Srd:JsonPath' was not found.");
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();
builder.Services.AddDaggerheartPersistence(connectionString);
builder.Services.AddCatalogServices();

var app = builder.Build();

// Seed SRD data on startup
var srdPath = Path.IsPathRooted(srdJsonPath)
    ? srdJsonPath
    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, srdJsonPath));
await Seed.SrdData(app.Services, srdPath);
await Seed.SampleCharacter(app.Services);

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
