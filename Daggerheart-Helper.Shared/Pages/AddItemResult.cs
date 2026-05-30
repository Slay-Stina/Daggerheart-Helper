namespace Daggerheart_Helper.Shared.Pages;

public sealed record AddItemResult
{
    public string ItemName { get; init; } = string.Empty;
    public Guid? CatalogId { get; init; }
    public string? SlotType { get; init; }
}
