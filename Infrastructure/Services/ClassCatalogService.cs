using Application.Dtos;
using Application.Services;
using Core.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class ClassCatalogQueries(DaggerheartDbContext context) : IClassCatalogQueries
{
    private readonly DbSet<GameClass> _classes = context.GameClasses;

    public List<GameClass> GetAll() => _classes
        .AsNoTracking()
        .Include(x => x.Subclasses).ThenInclude(s => s.Foundation)
        .Include(x => x.Subclasses).ThenInclude(s => s.Specialization)
        .Include(x => x.Subclasses).ThenInclude(s => s.Mastery)
        .Include(x => x.ClassFeatures)
        .Include(x => x.SuggestedWeapons)
        .Include(x => x.SuggestedArmor)
        .AsSplitQuery()
        .ToList();

    public GameClass GetById(Guid id) =>
        _classes.AsNoTracking().FirstOrDefault(x => x.Id == id)
        ?? throw new KeyNotFoundException($"GameClass '{id}' was not found.");

    public Task<List<ClassCardSummary>> GetCardSummariesAsync() =>
        _classes
            .AsNoTracking()
            .Select(c => new ClassCardSummary(
                c.Id,
                c.Name,
                c.Description,
                c.Domain1,
                c.Domain2,
                c.BaseEvasion,
                c.BaseHealth,
                c.ClassFeatures.Select(f => new FeatureSummary(f.Id, f.Name, f.Description)).ToList(),
                new FeatureSummary(c.HopeFeature.Id, c.HopeFeature.Name, c.HopeFeature.Description)))
            .ToListAsync();

    public Task<List<SubclassSummary>> GetSubclassesAsync(Guid classId) =>
        _classes
            .AsNoTracking()
            .Where(c => c.Id == classId)
            .SelectMany(c => c.Subclasses)
            .Select(s => new SubclassSummary(
                s.Id, s.Name, s.Description,
                new FeatureSummary(s.Foundation.Id, s.Foundation.Name, s.Foundation.Description),
                new FeatureSummary(s.Specialization.Id, s.Specialization.Name, s.Specialization.Description),
                new FeatureSummary(s.Mastery.Id, s.Mastery.Name, s.Mastery.Description),
                s.SpellCastingTraitType))
            .ToListAsync();

    public Task<ClassSetupData> GetClassSetupDataAsync(Guid classId) =>
        _classes
            .AsNoTracking()
            .Where(c => c.Id == classId)
            .Select(c => new ClassSetupData(
                c.SuggestedTraits,
                c.BackgroundQuestions,
                c.SuggestedArmor != null ? c.SuggestedArmor.Id : (Guid?)null,
                c.SuggestedWeapons.Select(w => w.Id).ToList(),
                c.Items))
            .FirstAsync();
}
