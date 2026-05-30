using Core.Enums;

namespace Application.Dtos;

public sealed record HeritageSummary(
    Guid Id,
    string Name,
    string Description,
    HeritageType Type,
    List<FeatureSummary> Features);
