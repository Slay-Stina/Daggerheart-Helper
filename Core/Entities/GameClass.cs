using Core.Enums;
using Core.ValueObjects;

namespace Core.Entities;

public class GameClass
{
    public Guid Id { get; init; }
    public string Name { get; init; } = String.Empty;
    public string Description { get; init; } = String.Empty;
    public int BaseEvasion { get; init; }
    public int BaseHealth { get; init; }
    public DomainType Domain1 { get; init; }
    public DomainType Domain2 { get; init; }
    public TraitScores SuggestedTraits { get; init; } = new( 0,0,0,0,0,0);
    public Armor SuggestedArmor { get; init; } = new();
    public List<Weapon> SuggestedWeapons { get; init; } = new();
    public List<Subclass> Subclasses { get; init; } = new();
    public List<Feature> Features { get; init; } = new();
    public Feature HopeFeature { get; init; } = new();
    public List<string> BackgroundQuestions { get; init; } = new();
    public List<string> ConnectionQuestions { get; init; } = new();
    public List<string> Items { get; init; } = new();
}
