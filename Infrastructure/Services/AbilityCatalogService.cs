using Application.Services;
using Core.Entities;
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
}
