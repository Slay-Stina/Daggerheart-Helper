using Core.Enums;

namespace Core.Value_Objects;

public record Damage(Dice Dice,int Bonus, DamageType Type);