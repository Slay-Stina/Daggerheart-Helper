namespace Srd.Ingestion.Domain;

public sealed record CommunityCard(
    string Name,
    string Description,
    IReadOnlyList<FeatureBlock> Feature,
    string Note) : HeritageCard(Name, Description, Feature, Note);