using Core.Enums;

namespace Srd.Ingestion.Domain;

public sealed record AdversaryCard(
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
    string Experience,
    string MotivesAndTactics,
    IReadOnlyList<FeatureBlock> Features);
