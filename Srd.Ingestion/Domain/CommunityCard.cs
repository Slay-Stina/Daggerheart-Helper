using Core.Enums;

namespace Srd.Ingestion.Domain;

public sealed record CommunityCard(
    string Name,
    string Description,
    IReadOnlyList<FeatureBlock> Feature,
    string Note,
    HeritageType HeritageType) : HeritageCard(Name, Description, Feature, Note, HeritageType);