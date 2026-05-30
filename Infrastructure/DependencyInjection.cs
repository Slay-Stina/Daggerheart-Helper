    using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Application.Services;
using Infrastructure.Services;
using Srd.Ingestion.Loading;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDaggerheartPersistence(this IServiceCollection services, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or whitespace.", nameof(connectionString));
        }

        services.AddDbContextFactory<DaggerheartDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<ISrdJsonLoader, SrdJsonLoader>();
        return services;
    }

    public static IServiceCollection AddCatalogServices(this IServiceCollection services)
    {
        services.AddScoped<IAbilityCatalogQueries, AbilityCatalogQueries>();
        services.AddScoped<IArmorCatalogQueries, ArmorCatalogQueries>();
        services.AddScoped<IClassCatalogQueries, ClassCatalogQueries>();
        services.AddScoped<IHeritageCatalogQueries, HeritageCatalogQueries>();
        services.AddScoped<IWeaponCatalogQueries, WeaponCatalogQueries>();
        services.AddScoped<ICharacterService, CharacterService>();
        return services;
    }
}
