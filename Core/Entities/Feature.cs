namespace Core.Entities;

public class Feature
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    public Guid? GameClassIdAsClassFeature { get; set; }
    public GameClass? GameClassAsClassFeature { get; init; }

    public Guid? GameClassIdAsHopeFeature { get; set; }
    public GameClass? GameClassAsHopeFeature { get; init; }

    public Guid? SubclassId { get; init; }
    public Subclass? Subclass { get; init; }

    public Guid? HeritageId { get; init; }
    public Heritage? Heritage { get; init; }

    public List<Weapon> Weapons { get; init; } = new();
    public List<Armor> Armors { get; init; } = new();
    public List<FeatureEffect> FeatureEffects { get; init; } = new();
}