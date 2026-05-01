using Core.Enums;

namespace Core.Entities;

public class Heritage
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Feature> Features { get; set; } = new();
    public string? Note { get; set; }
    public HeritageType HeritageType { get; set; }
}