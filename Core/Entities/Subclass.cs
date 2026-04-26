using Core.Enums;

namespace Core.Entities;

public class Subclass
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TraitType? SpellCastingTraitType { get; set; }
    
    public Guid GameClassId { get; set; }
    public GameClass GameClass { get; set; } = null!;
    public List<Feature> Features { get; set; } = new();
}