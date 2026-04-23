using Core.Enums;
using Core.ValueObjects;
using Srd.Ingestion.Parsing;
using Srd.Ingestion.Raw;
using Xunit;

namespace DaggerheartHelper.Tests.Srd.Ingestion.Tests.Parsing;

public class SrdParsersTests
{
    [Theory]
    [InlineData("1", 1)]
    [InlineData(" 4 ", 4)]
    public void ParseInt_ReturnsExpectedValue(string value, int expected)
    {
        var result = SrdParsers.ParseInt(value, "field");

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseThresholds_ReturnsMajorAndSevereValues()
    {
        var result = SrdParsers.ParseThresholds("5 / 11");

        Assert.Equal(new DamageThresholds(5, 11), result);
    }

    [Fact]
    public void ParseDamage_ReturnsDiceBonusAndKind()
    {
        var result = SrdParsers.ParseDamage("d10+3 phy");

        Assert.Equal(new Damage(new Dice(1, 10), 3, DamageType.Physical), result);
    }

    [Theory]
    [InlineData("One-Handed", Burden.OneHanded)]
    [InlineData("Two-Handed", Burden.TwoHanded)]
    public void ParseBurden_ReturnsExpectedEnumValue(string value, Burden expected)
    {
        Assert.Equal(expected, SrdParsers.ParseBurden(value));
    }

    [Theory]
    [InlineData("Melee", RangeType.Melee)]
    [InlineData("Very Close", RangeType.VeryClose)]
    [InlineData("Close", RangeType.Close)]
    [InlineData("Far", RangeType.Far)]
    [InlineData("Very Far", RangeType.VeryFar)]
    public void ParseRange_ReturnsExpectedEnumValue(string value, RangeType expected)
    {
        Assert.Equal(expected, SrdParsers.ParseRange(value));
    }

    [Fact]
    public void ParseFeatures_TrimsAndPreservesQuestion()
    {
        var features = SrdParsers.ParseFeatures(
            [
                new RawFeatureDto { Name = " Flexible ", Text = " +1 to Evasion ", Question = "Why? " }
            ]);

        Assert.Single(features);
        Assert.Equal(new global::Srd.Ingestion.Domain.FeatureBlock("Flexible", "+1 to Evasion", "Why?"), features[0]);
    }
}


