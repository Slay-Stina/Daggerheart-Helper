using Core.Enums;
using Core.ValueObjects;

namespace Core.Entities;

public class GameClass
{
    public Guid Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public string Description { get; set; } = String.Empty;
    public int BaseEvasion { get; set; }
    public int BaseHealth { get; set; }
    public List<DomainType> Domains { get; set; } = new();
    public TraitScores SuggestedTraits { get; set; } = new(0,0,0,0,0,0);
    public DamageThresholds StartingThresholds { get; set; } = new(0,0);
    public List<Subclass> Subclasses { get; set; } = new();
    public List<Feature> Features { get; set; } = new();
}
