namespace Core.Entities;

public class Feature
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public Guid? GameClassIdAsClassFeature { get; set; }
    public GameClass? GameClassAsClassFeature { get; set; }
    
    public Guid? GameClassIdAsHopeFeature { get; set; }
    public GameClass? GameClassAsHopeFeature { get; set; }
    
    public Guid? SubclassId { get; set; }
    public Subclass? Subclass { get; set; }
    
    public Guid? HeritageId { get; set; }
    public Heritage? Heritage { get; set; }
    
    public List<Weapon> Weapons { get; set; } = new();
    public List<Armor> Armors { get; set; } = new();
    public List<FeatureEffect> FeatureEffects { get; set; } = new();
}