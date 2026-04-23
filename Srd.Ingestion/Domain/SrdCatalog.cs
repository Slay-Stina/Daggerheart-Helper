namespace Srd.Ingestion.Domain;

public sealed record SrdCatalog(
    IReadOnlyList<ArmorCard> Armors,
    IReadOnlyList<WeaponCard> Weapons,
    IReadOnlyList<AbilityCard> Abilities);

