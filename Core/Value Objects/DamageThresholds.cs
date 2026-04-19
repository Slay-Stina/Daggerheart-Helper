namespace Core.ValueObjects;

public record DamageThresholds(int Minor, int Major, int Severe)
{
    public DamageThresholds(int Major, int Severe) : this(0, Major, Severe)
    {
    }
}
