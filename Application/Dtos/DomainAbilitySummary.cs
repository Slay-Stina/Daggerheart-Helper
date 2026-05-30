using Core.Enums;

namespace Application.Dtos;

public sealed record DomainAbilitySummary(
    Guid Id,
    string Title,
    DomainType DomainType,
    int Level,
    int RecallCost,
    AbilityType Type,
    string FeatureDescription);
