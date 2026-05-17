using Application.Services;
using Core.Entities;
using Core.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class HeritageCatalogQueries(DaggerheartDbContext context) : IHeritageCatalogQueries
{
    private readonly DbSet<Heritage> _heritages = context.Heritages;

    public List<Heritage> GetAll() => _heritages.AsNoTracking().ToList();

    public List<Heritage> GetAllAncestries() =>
        _heritages.AsNoTracking().Where(x => x.HeritageType == HeritageType.Ancestry).ToList();

    public List<Heritage> GetAllCommunities() =>
        _heritages.AsNoTracking().Where(x => x.HeritageType == HeritageType.Community).ToList();

    public Heritage GetById(Guid id) =>
        _heritages.AsNoTracking().FirstOrDefault(x => x.Id == id)
        ?? throw new KeyNotFoundException($"Heritage '{id}' was not found.");
}
