using Core.Enums;

namespace Application.Dtos;

public sealed record AbilitySummary(
    Guid Id,
    string Title,
    DomainType DomainType,
    int Level,
    int RecallCost,
    AbilityType Type,
    string FeatureDescription,
    bool IsVaulted = false);
