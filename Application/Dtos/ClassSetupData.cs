using Core.ValueObjects;

namespace Application.Dtos;

public sealed record ClassSetupData(
    TraitScores SuggestedTraits,
    List<string> BackgroundQuestions,
    Guid? SuggestedArmorId,
    List<Guid> SuggestedWeaponIds,
    List<string> ClassItems);
