using Core.Enums;

namespace Core.Entities;

public class Subclass
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public TraitType? SpellCastingTraitType { get; init; }
    
    public Guid FoundationId { get; init; }
    public Feature Foundation { get; init; } = null!;
    
    public Guid SpecializationId { get; init; }
    public Feature Specialization { get; init; } = null!;
    
    public Guid MasteryId { get; init; }
    public Feature Mastery { get; init; } = null!;
    
    public Guid GameClassId { get; init; }
    public GameClass? GameClass { get; init; }
}