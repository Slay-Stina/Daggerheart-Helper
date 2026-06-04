using Core.Enums;

namespace Application.Dtos;

public sealed record AdversarySummary(
    Guid Id,
    string Name,
    string Description,
    int Tier,
    AdversaryType Type,
    int Difficulty,
    int Hp,
    int Stress,
    string Thresholds,
    string Atk,
    string Attack,
    string Damage,
    string Range,
    List<FeatureSummary> Features);
