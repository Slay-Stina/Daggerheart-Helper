using Core.Enums;

namespace Srd.Ingestion.Domain;

public sealed record AncestryCard(
    string Name,
    string Description,
    IReadOnlyList<FeatureBlock> Feature,
    HeritageType HeritageType) : HeritageCard(Name, Description, Feature, null, HeritageType);