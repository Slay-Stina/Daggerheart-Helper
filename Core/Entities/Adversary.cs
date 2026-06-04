using Core.Enums;

namespace Core.Entities;

public class Adversary
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Tier { get; init; }
    public AdversaryType Type { get; init; }
    public int Difficulty { get; init; }
    public int Hp { get; init; }
    public int Stress { get; init; }
    public string Thresholds { get; init; } = string.Empty;
    public string Atk { get; init; } = string.Empty;
    public string Attack { get; init; } = string.Empty;
    public string Damage { get; init; } = string.Empty;
    public string Range { get; init; } = string.Empty;
    public string Experience { get; init; } = string.Empty;
    public string MotivesAndTactics { get; init; } = string.Empty;
    public List<Feature> Features { get; init; } = new();
}
