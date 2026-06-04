using Application.Dtos;
using Application.Services;
using Core.Entities;
using Core.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class AdversaryCatalogQueries(DaggerheartDbContext context) : IAdversaryCatalogQueries
{
    private readonly DbSet<Adversary> _adversaries = context.Adversaries;

    public Task<List<AdversarySummary>> GetAllAsync() =>
        _adversaries
            .AsNoTracking()
            .Include(a => a.Features)
            .OrderBy(a => a.Tier)
            .ThenBy(a => a.Name)
            .Select(a => MapToSummary(a))
            .ToListAsync();

    public Task<List<AdversarySummary>> SearchAsync(
        string? name,
        AdversaryType? type,
        int? tier,
        int? difficultyMin,
        int? difficultyMax)
    {
        var query = _adversaries.AsNoTracking().Include(a => a.Features).AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(a => EF.Functions.Like(a.Name, $"%{name}%"));

        if (type.HasValue)
            query = query.Where(a => a.Type == type.Value);

        if (tier.HasValue)
            query = query.Where(a => a.Tier == tier.Value);

        if (difficultyMin.HasValue)
            query = query.Where(a => a.Difficulty >= difficultyMin.Value);

        if (difficultyMax.HasValue)
            query = query.Where(a => a.Difficulty <= difficultyMax.Value);

        return query
            .OrderBy(a => a.Tier)
            .ThenBy(a => a.Name)
            .Select(a => MapToSummary(a))
            .ToListAsync();
    }

    public async Task<AdversarySummary?> GetByIdAsync(Guid id)
    {
        var adversary = await _adversaries
            .AsNoTracking()
            .Include(a => a.Features)
            .FirstOrDefaultAsync(a => a.Id == id);

        return adversary is null ? null : MapToSummary(adversary);
    }

    private static AdversarySummary MapToSummary(Adversary a) => new(
        a.Id,
        a.Name,
        a.Description,
        a.Tier,
        a.Type,
        a.Difficulty,
        a.Hp,
        a.Stress,
        a.Thresholds,
        a.Atk,
        a.Attack,
        a.Damage,
        a.Range,
        a.MotivesAndTactics,
        a.Experience,
        a.Features.Select(f => new FeatureSummary(f.Id, f.Name, f.Description)).ToList());
}
