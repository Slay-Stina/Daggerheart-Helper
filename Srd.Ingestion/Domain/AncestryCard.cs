using Core.Enums;

namespace Srd.Ingestion.Domain;

public sealed record AncestryCard(
    string Name,
    string Description,
    IReadOnlyList<FeatureBlock> Features,
    HeritageType HeritageType) : HeritageCard(Name, Description, Features, null, HeritageType);