using Core.Enums;

namespace Core.ValueObjects;

public record Damage(Dice Dice,int Bonus, DamageType Type);
