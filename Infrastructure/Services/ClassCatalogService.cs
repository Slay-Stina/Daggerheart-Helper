using Application.Services;
using Core.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class ClassCatalogQueries(DaggerheartDbContext context) : IClassCatalogQueries
{
    private readonly DbSet<GameClass> _classes = context.GameClasses;

    public List<GameClass> GetAll() => _classes.AsNoTracking().ToList();

    public GameClass GetById(Guid id) =>
        _classes.AsNoTracking().FirstOrDefault(x => x.Id == id)
        ?? throw new KeyNotFoundException($"GameClass '{id}' was not found.");
}
