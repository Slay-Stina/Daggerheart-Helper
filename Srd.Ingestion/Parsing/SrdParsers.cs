using System.Text.RegularExpressions;
using Core.Enums;
using Srd.Ingestion.Domain;
using Srd.Ingestion.Raw;
using Core.ValueObjects;

namespace Srd.Ingestion.Parsing;

public static partial class SrdParsers
{
    [GeneratedRegex(@"^(?<count>\d*)d(?<sides>\d+)(?<bonus>[+-]\d+)?\s+(?<kind>phy|mag)(?:\s+or\s+(?<altkind>phy|mag))?$", RegexOptions.IgnoreCase)]
    private static partial Regex DamageRegex();

    public static int ParseInt(string value, string fieldName)
    {
        if (!int.TryParse(value, out var parsed))
        {
            throw new FormatException($"Invalid integer for '{fieldName}': '{value}'.");
        }

        return parsed;
    }

    public static DamageThresholds ParseThresholds(string value)
    {
        var chunks = value.Split('/');
        if (chunks.Length != 2)
        {
            throw new FormatException($"Invalid threshold pair: '{value}'.");
        }

        var major = ParseInt(chunks[0].Trim(), "thresholds.major");
        var severe = ParseInt(chunks[1].Trim(), "thresholds.severe");
        return new DamageThresholds(major, severe);
    }

    public static Damage ParseDamage(string value)
    {
        var parsed = ParseDamageParts(value);

        var kind = parsed.Kind.Equals("mag", StringComparison.OrdinalIgnoreCase)
            ? DamageType.Magic
            : DamageType.Physical;

        return new Damage(parsed.Dice, parsed.Bonus, kind);
    }

    public static Damage ParseDamage(string value, DamageType damageType)
    {
        var parsed = ParseDamageParts(value);
        return new Damage(parsed.Dice, parsed.Bonus, damageType);
    }

    public static FeatureBlock ParseFeature(RawFeatureDto feature)
    {
        return new FeatureBlock(feature.Name.Trim(), feature.Text.Trim());
    }

    public static IReadOnlyList<FeatureBlock> ParseFeatures(List<RawFeatureDto>? features)
    {
        if (features is null || features.Count == 0)
        {
            return [];
        }

        return features
            .Select(ParseFeature).ToList();
    }

    public static int ParseTier(string value) => ParseInt(value, "tier");

    public static Burden ParseBurden(string value) => value.Trim() switch
    {
        "One-Handed" => Burden.OneHanded,
        "Two-Handed" => Burden.TwoHanded,
        _ => throw new FormatException($"Unsupported burden: '{value}'.")
    };

    public static WeaponPriority ParseWeaponPriority(string value) => value.Trim() switch
    {
        "Primary" => WeaponPriority.Primary,
        "Secondary" => WeaponPriority.Secondary,
        _ => throw new FormatException($"Unsupported weapon priority: '{value}'.")
    };

    public static RangeType ParseRange(string value) => value.Trim() switch
    {
        "Melee" => RangeType.Melee,
        "Very Close" => RangeType.VeryClose,
        "Close" => RangeType.Close,
        "Far" => RangeType.Far,
        "Very Far" => RangeType.VeryFar,
        _ => throw new FormatException($"Unsupported range: '{value}'.")
    };

    public static DamageType ParseDamageKind(string value) => value.Trim() switch
    {
        "Physical" => DamageType.Physical,
        "Magical" => DamageType.Magic,
        _ => throw new FormatException($"Unsupported damage kind: '{value}'.")
    };

    public static TraitType ParseTrait(string value) => ParseEnum<TraitType>(value, "trait");

    public static DomainType ParseDomain(string value) => ParseEnum<DomainType>(value, "domain");

    public static AbilityType ParseAbilityType(string value) => ParseEnum<AbilityType>(value, "type");

    private static TEnum ParseEnum<TEnum>(string value, string fieldName)
        where TEnum : struct, Enum
    {
        if (!string.IsNullOrEmpty(value) && Enum.TryParse<TEnum>(value.Trim(), ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        throw new FormatException($"Unsupported {fieldName}: '{value}'.");
    }

    private static (Dice Dice, int Bonus, string Kind) ParseDamageParts(string value)
    {
        var match = DamageRegex().Match(value.Trim());
        if (!match.Success)
        {
            throw new FormatException($"Invalid damage expression: '{value}'.");
        }

        var diceCount = string.IsNullOrWhiteSpace(match.Groups["count"].Value)
            ? 1
            : ParseInt(match.Groups["count"].Value, "damage.count");

        var sides = ParseInt(match.Groups["sides"].Value, "damage.sides");
        var bonus = string.IsNullOrWhiteSpace(match.Groups["bonus"].Value)
            ? 0
            : ParseInt(match.Groups["bonus"].Value, "damage.bonus");

        return (new Dice(diceCount, sides), bonus, match.Groups["kind"].Value);
    }

    public static List<string> ParseItems(string rawItems) => rawItems.Split(" or a ").ToList();

    public static List<string> ParseQuestions(List<RawQuestion>? rawQuestions) =>
        (rawQuestions is null || rawQuestions.Count == 0) ? [] :
            rawQuestions.Select(question => question.Text.Trim()).ToList();

    public static TraitScores ParseTraitScores(string rawSuggestedTraits)
    {
        var parsedTraits = rawSuggestedTraits.Split(',')
            .Select(s => ParseInt(s.Trim(), "trait"))
            .ToArray();
        return parsedTraits.Length == 6 
            ? new TraitScores(
            parsedTraits[0],
            parsedTraits[1],
            parsedTraits[2],
            parsedTraits[3],
            parsedTraits[4],
            parsedTraits[5]) 
            : throw new FormatException($"Invalid traits '{rawSuggestedTraits}'.");
    }
}



