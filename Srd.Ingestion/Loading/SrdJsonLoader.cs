using System.Text.Json;
using Srd.Ingestion.Domain;
using Srd.Ingestion.Parsing;
using Srd.Ingestion.Raw;

namespace Srd.Ingestion.Loading;

public sealed class SrdJsonLoader : ISrdJsonLoader
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<SrdCatalog> LoadAsync(string jsonDirectoryPath, CancellationToken cancellationToken = default)
    {
        var armors = await LoadFileAsync<RawArmorDto>(jsonDirectoryPath, "armor.json", cancellationToken);
        var weapons = await LoadFileAsync<RawWeaponDto>(jsonDirectoryPath, "weapons.json", cancellationToken);
        var abilities = await LoadFileAsync<RawAbilityDto>(jsonDirectoryPath, "abilities.json", cancellationToken);

        return new SrdCatalog(
            armors.Select(ToArmorCard).ToList(),
            weapons.Select(ToWeaponCard).ToList(),
            abilities.Select(ToAbilityCard).ToList());
    }

    private static async Task<List<T>> LoadFileAsync<T>(string directory, string fileName, CancellationToken cancellationToken)
    {
        var fullPath = Path.Combine(directory, fileName);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Could not find SRD file '{fileName}' in '{directory}'.", fullPath);
        }

        await using var stream = File.OpenRead(fullPath);
        var data = await JsonSerializer.DeserializeAsync<List<T>>(stream, SerializerOptions, cancellationToken);

        if (data is null)
        {
            throw new InvalidDataException($"File '{fullPath}' did not deserialize into a JSON list.");
        }

        return data;
    }

    private static ArmorCard ToArmorCard(RawArmorDto raw)
    {
        return new ArmorCard(
            raw.Name.Trim(),
            SrdParsers.ParseTier(raw.Tier),
            SrdParsers.ParseInt(raw.BaseScore, "base_score"),
            SrdParsers.ParseThresholds(raw.BaseThresholds),
            SrdParsers.ParseFeatures(raw.Features));
    }

    private static WeaponCard ToWeaponCard(RawWeaponDto raw)
    {
        return new WeaponCard(
            raw.Name.Trim(),
            SrdParsers.ParseTier(raw.Tier),
            SrdParsers.ParseBurden(raw.Burden),
            SrdParsers.ParseWeaponPriority(raw.PrimaryOrSecondary),
            SrdParsers.ParseTrait(raw.Trait),
            SrdParsers.ParseRange(raw.Range),
            SrdParsers.ParseDamageKind(raw.PhysicalOrMagical),
            SrdParsers.ParseDamage(raw.Damage, SrdParsers.ParseDamageKind(raw.PhysicalOrMagical)),
            SrdParsers.ParseFeatures(raw.Features));
    }

    private static AbilityCard ToAbilityCard(RawAbilityDto raw)
    {
        return new AbilityCard(
            raw.Name.Trim(),
            SrdParsers.ParseDomain(raw.Domain),
            SrdParsers.ParseInt(raw.Level, "level"),
            SrdParsers.ParseInt(raw.Recall, "recall"),
            SrdParsers.ParseAbilityType(raw.Type),
            raw.Text.Trim());
    }
}

