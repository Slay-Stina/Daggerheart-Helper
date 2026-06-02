using System.Text.Json;
using Core.Entities;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Persistence;

public class DaggerheartDbContext(DbContextOptions<DaggerheartDbContext> options) : DbContext(options)
{
    public DbSet<Ability> Abilities => Set<Ability>();
    public DbSet<Armor> Armors => Set<Armor>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<Feature> Features => Set<Feature>();
    public DbSet<FeatureEffect> FeatureEffects => Set<FeatureEffect>();
    public DbSet<GameClass> GameClasses => Set<GameClass>();
    public DbSet<Subclass> Subclasses => Set<Subclass>();
    public DbSet<Weapon> Weapons => Set<Weapon>();
    public DbSet<Heritage> Heritages => Set<Heritage>();
    public DbSet<CharacterAbility> CharacterAbilities => Set<CharacterAbility>();
    public DbSet<Item> Items => Set<Item>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>().HaveConversion<string>();
        configurationBuilder.Properties<List<string>>().HaveConversion<JsonStringListConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ability>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
            entity.Property(x => x.FeatureDescription).IsRequired();
            entity.HasIndex(x => new { x.DomainType, x.Level });
        });

        modelBuilder.Entity<Armor>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.ArmorScore).IsRequired();
            entity.HasOne(x => x.Feature)
                .WithMany(f => f.Armors)
                .HasForeignKey(x => x.FeatureId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.OwnsOne(x => x.DamageThresholds, owned =>
            {
                owned.Property(x => x.Major).HasColumnName("MajorThreshold").HasDefaultValue(0);
                owned.Property(x => x.Severe).HasColumnName("SevereThreshold").HasDefaultValue(0);
            });
        });

        modelBuilder.Entity<Character>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Level).HasDefaultValue(1);
            entity.Ignore(x => x.Proficiency);
            entity.Property(x => x.Experiences)
                .HasConversion<JsonStringListConverter>()
                .Metadata.SetValueComparer(GetListValueComparer());
            entity.Property(x => x.BackgroundAnswers)
                .HasConversion<JsonStringListConverter>()
                .Metadata.SetValueComparer(GetListValueComparer());
            entity.HasMany(x => x.Inventory)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "CharacterItems",
                    r => r.HasOne<Item>().WithMany().HasForeignKey("ItemId").OnDelete(DeleteBehavior.Cascade),
                    l => l.HasOne<Character>().WithMany().HasForeignKey("CharacterId").OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("CharacterId", "ItemId");
                        j.ToTable("CharacterItems");
                    });

            entity.Property(x => x.RowVersion)
                .HasDefaultValue(Array.Empty<byte>())
                .IsRowVersion();

            entity.OwnsOne(x => x.Traits, owned =>
            {
                owned.Property(x => x.Agility).HasColumnName(nameof(TraitScores.Agility));
                owned.Property(x => x.Strength).HasColumnName(nameof(TraitScores.Strength));
                owned.Property(x => x.Finesse).HasColumnName(nameof(TraitScores.Finesse));
                owned.Property(x => x.Instinct).HasColumnName(nameof(TraitScores.Instinct));
                owned.Property(x => x.Presence).HasColumnName(nameof(TraitScores.Presence));
                owned.Property(x => x.Knowledge).HasColumnName(nameof(TraitScores.Knowledge));
            });

            entity.OwnsOne(x => x.DamageThresholds, owned =>
            {
                owned.Property(x => x.Major).HasColumnName("DamageThresholdMajor").HasDefaultValue(0);
                owned.Property(x => x.Severe).HasColumnName("DamageThresholdSevere").HasDefaultValue(0);
            });

            entity.OwnsOne(x => x.HitPoints, owned =>
            {
                owned.Property(x => x.Current).HasColumnName("HitPointsCurrent");
                owned.Property(x => x.Max).HasColumnName("HitPointsMax");
            });

            entity.OwnsOne(x => x.Stress, owned =>
            {
                owned.Property(x => x.Current).HasColumnName("StressCurrent");
                owned.Property(x => x.Max).HasColumnName("StressMax");
            });

            entity.OwnsOne(x => x.Hope, owned =>
            {
                owned.Property(x => x.Current).HasColumnName("HopeCurrent");
                owned.Property(x => x.Max).HasColumnName("HopeMax");
            });

            entity.OwnsOne(x => x.ArmorSlots, owned =>
            {
                owned.Property(x => x.Current).HasColumnName("ArmorSlotsCurrent");
                owned.Property(x => x.Max).HasColumnName("ArmorSlotsMax");
            });

            entity.HasOne(x => x.GameClass)
                .WithMany()
                .HasForeignKey(x => x.GameClassId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Subclass)
                .WithMany()
                .HasForeignKey(x => x.SubclassId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Multiclass)
                .WithMany()
                .HasForeignKey(x => x.MulticlassId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.MulticlassSubclass)
                .WithMany()
                .HasForeignKey(x => x.MulticlassSubclassId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.EquippedArmor)
                .WithMany()
                .HasForeignKey(x => x.EquippedArmorId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.PrimaryWeapon)
                .WithMany()
                .HasForeignKey(x => x.PrimaryWeaponId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.SecondaryWeapon)
                .WithMany()
                .HasForeignKey(x => x.SecondaryWeaponId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(x => x.CharacterAbilities)
                .WithOne(x => x.Character)
                .HasForeignKey(x => x.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(x => x.Ancestry)
                .WithMany()
                .HasForeignKey(x => x.AncestryId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(x => x.Community)
                .WithMany()
                .HasForeignKey(x => x.CommunityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Feature>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Description).IsRequired();

            entity.HasOne(x => x.GameClassAsClassFeature)
                .WithMany(x => x.ClassFeatures)
                .HasForeignKey(x => x.GameClassIdAsClassFeature)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.GameClassAsHopeFeature)
                .WithOne(x => x.HopeFeature)
                .HasForeignKey<GameClass>(x => x.HopeFeatureId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Subclass)
                .WithMany()
                .HasForeignKey(x => x.SubclassId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Heritage)
                .WithMany(x => x.Features)
                .HasForeignKey(x => x.HeritageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Weapons)
                .WithOne(x => x.Feature)
                .HasForeignKey(x => x.FeatureId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(x => x.Armors)
                .WithOne(x => x.Feature)
                .HasForeignKey(x => x.FeatureId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(x => x.FeatureEffects)
                .WithOne(x => x.Feature)
                .HasForeignKey(x => x.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FeatureEffect>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Condition).HasMaxLength(200);
            entity.Property(x => x.Description).HasMaxLength(1000);

            entity.HasIndex(x => x.FeatureId);
            entity.HasIndex(x => new { x.EffectType, x.Target });

            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_FeatureEffects_TraitTargetRequiresTraitType",
                    "\"Target\" <> 'Trait' OR \"TraitType\" IS NOT NULL");
            });
        });

        modelBuilder.Entity<GameClass>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Description).IsRequired();
            entity.Property(x => x.BaseEvasion).IsRequired();
            entity.Property(x => x.BaseHealth).IsRequired();
            entity.Property(x => x.Domain1).IsRequired();
            entity.Property(x => x.Domain2).IsRequired();

            entity.OwnsOne(x => x.SuggestedTraits, owned =>
            {
                owned.Property(x => x.Agility).HasColumnName($"{nameof(GameClass.SuggestedTraits)}_{nameof(TraitScores.Agility)}");
                owned.Property(x => x.Strength).HasColumnName($"{nameof(GameClass.SuggestedTraits)}_{nameof(TraitScores.Strength)}");
                owned.Property(x => x.Finesse).HasColumnName($"{nameof(GameClass.SuggestedTraits)}_{nameof(TraitScores.Finesse)}");
                owned.Property(x => x.Instinct).HasColumnName($"{nameof(GameClass.SuggestedTraits)}_{nameof(TraitScores.Instinct)}");
                owned.Property(x => x.Presence).HasColumnName($"{nameof(GameClass.SuggestedTraits)}_{nameof(TraitScores.Presence)}");
                owned.Property(x => x.Knowledge).HasColumnName($"{nameof(GameClass.SuggestedTraits)}_{nameof(TraitScores.Knowledge)}");
            });

            entity.Property(x => x.BackgroundQuestions)
                .HasConversion<JsonStringListConverter>()
                .Metadata.SetValueComparer(GetListValueComparer());
            entity.Property(x => x.ConnectionQuestions)
                .HasConversion<JsonStringListConverter>()
                .Metadata.SetValueComparer(GetListValueComparer());
            entity.Property(x => x.Items)
                .HasConversion<JsonStringListConverter>()
                .Metadata.SetValueComparer(GetListValueComparer());

            entity.HasOne(x => x.SuggestedArmor)
                .WithMany()
                .HasForeignKey("SuggestedArmorId")
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasMany(x => x.ClassFeatures)
                .WithOne(x => x.GameClassAsClassFeature)
                .HasForeignKey(x => x.GameClassIdAsClassFeature)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.SuggestedWeapons)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "GameClassSuggestedWeapons",
                    r => r.HasOne<Weapon>().WithMany().HasForeignKey("WeaponId").OnDelete(DeleteBehavior.Cascade),
                    l => l.HasOne<GameClass>().WithMany().HasForeignKey("GameClassId").OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("GameClassId", "WeaponId");
                        j.ToTable("GameClassSuggestedWeapons");
                    });

            entity.HasMany(x => x.Subclasses)
                .WithOne(x => x.GameClass)
                .HasForeignKey(x => x.GameClassId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Subclass>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Description).IsRequired();
            
            entity.HasOne(x => x.Foundation)
                .WithMany()
                .HasForeignKey(x => x.FoundationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Specialization)
                .WithMany()
                .HasForeignKey(x  => x.SpecializationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Mastery)
                .WithMany()
                .HasForeignKey(x =>  x.MasteryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.GameClass)
                .WithMany(x => x.Subclasses)
                .HasForeignKey(x => x.GameClassId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Heritage>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Description).IsRequired();
            entity.Property(x => x.HeritageType).IsRequired();
            
            entity.HasMany(x => x.Features)
                .WithOne(x => x.Heritage)
                .HasForeignKey(x => x.HeritageId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<Weapon>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.HasOne(x => x.Feature)
                .WithMany(f =>  f.Weapons)
                .HasForeignKey(x => x.FeatureId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.OwnsOne(x => x.Damage, damage =>
            {
                damage.Property(x => x.NumberOfDice).HasColumnName("DamageDiceCount").HasDefaultValue(1);
                damage.Property(x => x.NumberOfSides).HasColumnName("DamageDiceSides").HasDefaultValue(1);
                damage.Property(x => x.Bonus).HasColumnName("DamageBonus").HasDefaultValue(0);
                damage.Property(x => x.Type).HasColumnName("DamageType").IsRequired();
            });
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Description).HasMaxLength(2000);
        });

        modelBuilder.Entity<CharacterAbility>(entity =>
        {
            entity.HasKey(x => new { x.CharacterId, x.AbilityId });
            entity.ToTable("CharacterAbilities");
            entity.Property(x => x.IsVaulted).HasDefaultValue(false);

            entity.HasOne(x => x.Character)
                .WithMany(x => x.CharacterAbilities)
                .HasForeignKey(x => x.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Ability)
                .WithMany(x => x.CharacterAbilities)
                .HasForeignKey(x => x.AbilityId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static ValueComparer<List<string>> GetListValueComparer()
    {
        return new ValueComparer<List<string>>(
            (a, b) => (a ?? new List<string>()).SequenceEqual(b ?? new List<string>()),
            c => c.Aggregate(0, (h, v) => HashCode.Combine(h, v.GetHashCode())),
            c => c.ToList()
        );
    }

    private class JsonStringListConverter() : ValueConverter<List<string>, string>(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!) ?? new List<string>());
}
