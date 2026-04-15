using Core.Enums;
using Core.Value_Objects;

namespace Core.Entities;

public class GameClass
{
    public Guid Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public string Description { get; set; } = String.Empty;
    public int BaseEvasion { get; set; }
    public int BaseHealth { get; set; }
    public Domain Domains { get; set; }
    public DamageThresholds StartingThresholds { get; set; } = null!;
    public List<Subclass> Subclasses { get; set; } = new();
    public List<ClassFeature> Features { get; set; } = new();
}