namespace Srd.Ingestion.Domain;

public sealed record AncestryCard(
    string Name,
    string Description,
    IReadOnlyList<FeatureBlock> Feature) : HeritageCard(Name, Description, Feature, null);