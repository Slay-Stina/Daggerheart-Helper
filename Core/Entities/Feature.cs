namespace Core.Entities;

public class Feature
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsHopeFeature { get; set; }
    public bool IsWeaponFeature { get; set; }
}