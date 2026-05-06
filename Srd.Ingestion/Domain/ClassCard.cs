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
    FeatureBlock ClassFeature,
    FeatureBlock HopeFeature,
    List<string> Items,
    List<string> BackgroundQuestions,
    List<string> ConnectionQuestions,
    ArmorCard SuggestedArmor,
    IReadOnlyList<WeaponCard> SuggestedWeapons);