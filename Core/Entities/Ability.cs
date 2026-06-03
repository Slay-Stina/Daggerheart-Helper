using Core.Enums;

namespace Core.Entities;

public class Ability
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public DomainType DomainType { get; init; }
    public int Level { get; init; }
    public int RecallCost { get; init; }
    public AbilityType Type { get; init; }
    public string FeatureDescription { get; init; } = string.Empty;
    public List<CharacterAbility> CharacterAbilities { get; set; } = new();
}   