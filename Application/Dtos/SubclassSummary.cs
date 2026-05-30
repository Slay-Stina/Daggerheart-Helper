using Core.Enums;

namespace Application.Dtos;

public sealed record SubclassSummary(
    Guid Id,
    string Name,
    string Description,
    FeatureSummary Foundation,
    TraitType? SpellCastingTraitType = null);
