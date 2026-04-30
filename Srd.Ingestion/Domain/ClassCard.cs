using Core.Enums;
using Core.ValueObjects;

namespace Srd.Ingestion.Domain;

public sealed record ClassCard(
    string Name,
    string Description,
    DomainType Domain1,
    DomainType Domain2,
    int BaseHp,
    int BaseEvasion,
    TraitScores SuggestedTraitScores,
    IReadOnlyList<SubclassCard> Subclasses,
    IReadOnlyList<FeatureBlock> Features,
    FeatureBlock HopeFeature,
    IReadOnlyList<string> Items,
    IReadOnlyList<string> BackgroundQuestions,
    IReadOnlyList<string> ConnectionQuestions,
    ArmorCard SuggestedArmor,
    IReadOnlyList<WeaponCard> SuggestedWeapons);