using Core.Enums;

namespace Core.ValueObjects;

public record Damage(int NumberOfDice, int NumberOfSides, int Bonus, DamageType Type)
{
    public Dice Dice => new(NumberOfDice, NumberOfSides);
}
