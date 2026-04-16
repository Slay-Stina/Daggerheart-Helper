using Core.Enums;

namespace Core.Entities;

public class Ability
{
    public Guid Id { get; init; }
    public string Title { get; set; } = string.Empty;
    public Domain Domain { get; set; }
    public int Level { get; set; }
    public int RecallCost { get; set; }
    public AbilityType Type { get; set; }
    public string FeatureDescription { get; set; } = string.Empty;
}   