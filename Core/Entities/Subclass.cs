using Core.Enums;

namespace Core.Entities;

public class Subclass
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TraitType? SpellCastingTraitType { get; set; }
    
    public int FoundationId { get; set; }
    public Feature Foundation { get; set; } = null!;
    
    public int SpecializationId { get; set; }
    public Feature Specialization { get; set; } = null!;
    
    public int MasteryId { get; set; }
    public Feature Mastery { get; set; } = null!;
    
    public Guid GameClassId { get; set; }
    public GameClass? GameClass { get; set; }
}