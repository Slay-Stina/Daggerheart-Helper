namespace Core.Entities;

public class CharacterAbility
{
    public Guid CharacterId { get; set; }
    public Character Character { get; set; } = null!;
    public Guid AbilityId { get; set; }
    public Ability Ability { get; set; } = null!;
    public bool IsVaulted { get; set; }
}
