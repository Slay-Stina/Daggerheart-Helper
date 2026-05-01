using Core.Enums;

namespace Srd.Ingestion.Domain;

public sealed record SubclassCard(
    string Name,
    string Description,
    TraitType? SpellcastTrait,
    IReadOnlyList<FeatureBlock> Features
    );