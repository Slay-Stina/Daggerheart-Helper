using Core.Enums;

namespace Core.Entities;

public class Heritage
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public List<Feature> Features { get; init; } = new();
    public string? Note { get; init; }
    public HeritageType HeritageType { get; init; }
}