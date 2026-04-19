namespace Core.ValueObjects;

public record ResourcePool(int Current, int Max)
{
    public bool IsFull => Current >= Max;
    public bool IsEmpty => Current <= 0;
}
