namespace Core.Value_Objects;

public record DamageThresholds(int Minor, int Major, int Severe)
{
    public DamageThresholds(int Major, int Severe) : this(0, Major, Severe)
    {
    }
}

