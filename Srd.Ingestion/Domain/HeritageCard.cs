using Core.Enums;

namespace Srd.Ingestion.Domain;

public abstract record HeritageCard(
    string Name, 
    string Description, 
    IReadOnlyList<FeatureBlock> Features, 
    string? Note,
    HeritageType HeritageType);