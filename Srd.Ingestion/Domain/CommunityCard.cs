using Core.Enums;

namespace Srd.Ingestion.Domain;

public sealed record CommunityCard(
    string Name,
    string Description,
    IReadOnlyList<FeatureBlock> Features,
    string Note,
    HeritageType HeritageType) : HeritageCard(Name, Description, Features, Note, HeritageType);