namespace Core.Entities;

public class Feature
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<GameClass> GameClasses { get; set; } = new();
    public List<Subclass> Subclasses { get; set; } = new();
    public List<Weapon> Weapons { get; set; } = new();
    public List<Armor> Armors { get; set; } = new();
    public List<FeatureEffect> FeatureEffects { get; set; } = new();
}