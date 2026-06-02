using Application.Dtos;
using Application.Services;
using Core.Entities;
using Core.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class AbilityCatalogQueries(DaggerheartDbContext context) : IAbilityCatalogQueries
{
    private readonly DbSet<Ability> _abilities = context.Abilities;

    public List<Ability> GetAll() => _abilities.AsNoTracking().ToList();

    public Ability GetById(Guid id) =>
        _abilities.AsNoTracking().FirstOrDefault(x => x.Id == id)
        ?? throw new KeyNotFoundException($"Ability '{id}' was not found.");

    public Task<List<AbilitySummary>> GetByDomainAsync(DomainType domain, int level) =>
        _abilities
            .AsNoTracking()
            .Where(a => a.DomainType == domain && a.Level == level)
            .Select(a => new AbilitySummary(
                a.Id, a.Title, a.DomainType, a.Level, a.RecallCost, a.Type, a.FeatureDescription))
            .ToListAsync();
}
