using Core.Enums;
using Core.ValueObjects;

namespace Srd.Ingestion.Domain;

public sealed record ClassCard(
    string Name,
    string Description,
    IReadOnlyList<DomainType> Domains,
    int BaseHp,
    int BaseEvasion,
    TraitScores SuggestedTraitScores,
    IReadOnlyList<SubclassCard> Subclasses,
    IReadOnlyList<FeatureBlock> Features,
    IReadOnlyList<string> Items,
    IReadOnlyList<string> BackgroundQuestions,
    IReadOnlyList<string> ConnectionQuestions,
    ArmorCard SuggestedArmor,
    IReadOnlyList<WeaponCard> SuggestedWeapons);