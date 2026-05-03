using System.Text.Json;
using Core.Enums;
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
        var rawArmors = await LoadFileAsync<RawArmorDto>(jsonDirectoryPath, "armor.json", cancellationToken);
        var rawWeapons = await LoadFileAsync<RawWeaponDto>(jsonDirectoryPath, "weapons.json", cancellationToken);
        var rawAbilities = await LoadFileAsync<RawAbilityDto>(jsonDirectoryPath, "abilities.json", cancellationToken);
        var rawAncestries = await LoadFileAsync<RawAncestryDto>(jsonDirectoryPath, "ancestries.json", cancellationToken);
        var rawCommunities = await LoadFileAsync<RawCommunityDto>(jsonDirectoryPath, "communities.json", cancellationToken);
        var rawSubclasses = await LoadFileAsync<RawSubclassDto>(jsonDirectoryPath, "subclasses.json", cancellationToken);
        var rawClasses = await LoadFileAsync<RawClassDto>(jsonDirectoryPath, "classes.json", cancellationToken);
        var subclasses = rawSubclasses.Select(ToSubclassCard).ToList();
        var armors = rawArmors.Select(ToArmorCard).ToList();
        var weapons = rawWeapons.Select(ToWeaponCard).ToList();
        
        return new SrdCatalog(
            armors,
            weapons,
            rawAbilities.Select(ToAbilityCard).ToList(),
            rawAncestries.Select(ToAncestryCard).ToList(),
            rawCommunities.Select(ToCommunityCard).ToList(),
            subclasses,
            rawClasses.Select(raw => ToClassCard(raw, subclasses, weapons, armors)).ToList()
            );
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
    
    private static AncestryCard ToAncestryCard(RawAncestryDto raw)
    {
        return new AncestryCard(
            raw.Name.Trim(),
            raw.Description.Trim(),
            SrdParsers.ParseFeatures(raw.Features),
            HeritageType.Ancestry);
    }
    
    private static CommunityCard ToCommunityCard(RawCommunityDto raw)
    {
        return new CommunityCard(
            raw.Name.Trim(),
            raw.Description.Trim(),
            SrdParsers.ParseFeatures(raw.Feature),
            raw.Note.Trim(),
            HeritageType.Community);
    }
    
    private static ClassCard ToClassCard(RawClassDto raw, List<SubclassCard> subclasses, List<WeaponCard> weapons, List<ArmorCard> armors)
    {
        return new ClassCard(
            raw.Name.Trim(),
            raw.Description.Trim(),
            SrdParsers.ParseDomain(raw.Domain1),
            SrdParsers.ParseDomain(raw.Domain2),
            SrdParsers.ParseInt(raw.BaseHp, "hp"),
            SrdParsers.ParseInt(raw.BaseEvasion, "evasion"),
            SrdParsers.ParseTraitScores(raw.SuggestedTraits),
            subclasses.Where(s => s.Name == raw.SubClass1 || s.Name == raw.SubClass2).ToList(),
            SrdParsers.ParseFeatures(raw.Features),
            SrdParsers.ParseFeature(new RawFeatureDto() { Name = raw.HopeFeatureName, Text = raw.HopeFeatureText }),
            SrdParsers.ParseItems(raw.Items).ToList(),
            SrdParsers.ParseQuestions(raw.BackgroundQuestions),
            SrdParsers.ParseQuestions(raw.ConnectionQuestions),
            armors.Single(a => a.Name == raw.SuggestedArmor),
            weapons.Where( w => w.Name == raw.SuggestedPrimary || w.Name == raw.SuggestedSecondary).ToList()
            );
    }
    
    private static SubclassCard ToSubclassCard(RawSubclassDto raw)
    {
        var rawFeatures = raw.Foundation
            .Concat(raw.Specialization)
            .Concat(raw.Mastery).ToList();

        return new SubclassCard(
            raw.Name.Trim(),
            raw.Description.Trim(),
            string.IsNullOrEmpty(raw.SpellcastTrait) ? null : SrdParsers.ParseTrait(raw.SpellcastTrait),
            SrdParsers.ParseFeatures(rawFeatures));
    }
}

