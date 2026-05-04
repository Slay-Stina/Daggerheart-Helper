using Core.Enums;

namespace Core.Entities;

public class Subclass
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TraitType? SpellCastingTraitType { get; set; }
    public Feature Foundation { get; set; } = null!;
    public Feature Specialization { get; set; } = null!;
    public Feature Mastery { get; set; } = null!;

    public Guid GameClassId { get; set; }
    public GameClass GameClass { get; set; } = null!;
}