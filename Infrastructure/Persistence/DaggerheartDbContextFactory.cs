using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence;

public class DaggerheartDbContextFactory : IDesignTimeDbContextFactory<DaggerheartDbContext>
{
    public DaggerheartDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Data Source=daggerheart.db";

        var optionsBuilder = new DbContextOptionsBuilder<DaggerheartDbContext>();
        optionsBuilder.UseSqlite(connectionString);

        return new DaggerheartDbContext(optionsBuilder.Options);
    }
}
