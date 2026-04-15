using Core.Enums;

namespace Core.Entities;

public class Subclass
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TraitType SpellCastingTraitType { get; set; }
    
    public int GameClassId { get; set; } // Foreign Key
}