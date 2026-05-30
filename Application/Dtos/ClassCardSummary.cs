using Core.Enums;

namespace Application.Dtos;

public sealed record ClassCardSummary(
    Guid Id,
    string Name,
    string Description,
    DomainType Domain1,
    DomainType Domain2,
    int BaseEvasion,
    int BaseHealth,
    List<FeatureSummary> ClassFeatures,
    FeatureSummary HopeFeature);
