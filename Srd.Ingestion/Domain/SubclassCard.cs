using Core.Enums;

namespace Srd.Ingestion.Domain;

public sealed record SubclassCard(
    string Name,
    string Description,
    TraitType? SpellcastTrait,
    FeatureBlock Foundation,
    FeatureBlock Specialization,
    FeatureBlock Mastery);