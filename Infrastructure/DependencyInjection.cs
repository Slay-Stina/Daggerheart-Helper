using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

        services.AddDbContext<DaggerheartDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<ISrdJsonLoader, SrdJsonLoader>();
        return services;
    }
}
