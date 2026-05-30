using Application.Dtos;
using Application.Services;
using Core.Entities;
using Core.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class HeritageCatalogQueries(DaggerheartDbContext context) : IHeritageCatalogQueries
{
    private readonly DbSet<Heritage> _heritages = context.Heritages;

    public List<Heritage> GetAll() => _heritages.AsNoTracking().Include(x => x.Features).ToList();

    public List<Heritage> GetAllAncestries() =>
        _heritages.AsNoTracking().Include(x => x.Features).Where(x => x.HeritageType == HeritageType.Ancestry).ToList();

    public List<Heritage> GetAllCommunities() =>
        _heritages.AsNoTracking().Include(x => x.Features).Where(x => x.HeritageType == HeritageType.Community).ToList();

    public Heritage GetById(Guid id) =>
        _heritages.AsNoTracking().Include(x => x.Features).FirstOrDefault(x => x.Id == id)
        ?? throw new KeyNotFoundException($"Heritage '{id}' was not found.");

    public Task<List<HeritageSummary>> GetSummariesAsync(HeritageType type) =>
        _heritages
            .AsNoTracking()
            .Where(h => h.HeritageType == type)
            .Select(h => new HeritageSummary(
                h.Id,
                h.Name,
                h.Description,
                h.HeritageType,
                h.Features.Select(f => new FeatureSummary(f.Id, f.Name, f.Description)).ToList()))
            .ToListAsync();
}
