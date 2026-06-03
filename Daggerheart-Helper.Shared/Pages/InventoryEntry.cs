namespace Daggerheart_Helper.Shared.Pages;

public sealed record InventoryEntry
{
    public string Name { get; init; } = string.Empty;
    public Guid? CatalogId { get; init; }
    public string? SlotType { get; init; }
}
