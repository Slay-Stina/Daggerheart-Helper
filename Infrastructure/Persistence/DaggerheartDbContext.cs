using Core.Entities;
using Core.Enums;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Persistence;

public class DaggerheartDbContext : DbContext
{
    public DaggerheartDbContext(DbContextOptions<DaggerheartDbContext> options) : base(options)
    {
    }

    public DbSet<Ability> Abilities => Set<Ability>();
    public DbSet<Armor> Armors => Set<Armor>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<Feature> Features => Set<Feature>();
    public DbSet<FeatureEffect> FeatureEffects => Set<FeatureEffect>();
    public DbSet<GameClass> GameClasses => Set<GameClass>();
    public DbSet<Subclass> Subclasses => Set<Subclass>();
    public DbSet<Weapon> Weapons => Set<Weapon>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>().HaveConversion<string>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ability>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
            entity.Property(x => x.FeatureDescription).IsRequired();
            entity.HasIndex(x => new { x.Domain, x.Level });
        });

        modelBuilder.Entity<Armor>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.ArmorScore).IsRequired();
            entity.OwnsOne(x => x.DamageThresholds, owned =>
            {
                owned.Property(x => x.Minor).HasColumnName("MinorThreshold");
                owned.Property(x => x.Major).HasColumnName("MajorThreshold");
                owned.Property(x => x.Severe).HasColumnName("SevereThreshold");
            });
            entity.HasMany(x => x.Features)
                .WithMany(x => x.Armors)
                .UsingEntity<Dictionary<string, object>>(
                    "ArmorFeatures",
                    right => right
                        .HasOne<Feature>()
                        .WithMany()
                        .HasForeignKey("FeatureId")
                        .OnDelete(DeleteBehavior.Cascade),
                    left => left
                        .HasOne<Armor>()
                        .WithMany()
                        .HasForeignKey("ArmorId")
                        .OnDelete(DeleteBehavior.Cascade),
                    join =>
                    {
                        join.HasKey("ArmorId", "FeatureId");
                        join.ToTable("ArmorFeatures");
                    });
        });

        modelBuilder.Entity<Character>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Level).HasDefaultValue(1);
            entity.Ignore(x => x.Proficiency);

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
                owned.Property(x => x.Minor).HasColumnName("DamageThresholdMinor");
                owned.Property(x => x.Major).HasColumnName("DamageThresholdMajor");
                owned.Property(x => x.Severe).HasColumnName("DamageThresholdSevere");
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
        });

        modelBuilder.Entity<Feature>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Description).IsRequired();

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
            entity.Property(x => x.Domains)
                .HasConversion(
                    domains => string.Join(',', domains),
                    stored => string.IsNullOrWhiteSpace(stored)
                        ? new List<Domain>()
                        : stored.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(Enum.Parse<Domain>)
                            .ToList())
                .Metadata.SetValueComparer(new ValueComparer<List<Domain>>(
                    (left, right) => (left == null && right == null) || (left != null && right != null && left.SequenceEqual(right)),
                    domains => domains == null ? 0 : domains.Aggregate(0, (hash, domain) => HashCode.Combine(hash, domain.GetHashCode())),
                    domains => domains == null ? new List<Domain>() : domains.ToList()));
            entity.OwnsOne(x => x.SuggestedTraits, owned =>
            {
                owned.Property(x => x.Agility).HasColumnName($"{nameof(GameClass.SuggestedTraits)}_{nameof(TraitScores.Agility)}");
                owned.Property(x => x.Strength).HasColumnName($"{nameof(GameClass.SuggestedTraits)}_{nameof(TraitScores.Strength)}");
                owned.Property(x => x.Finesse).HasColumnName($"{nameof(GameClass.SuggestedTraits)}_{nameof(TraitScores.Finesse)}");
                owned.Property(x => x.Instinct).HasColumnName($"{nameof(GameClass.SuggestedTraits)}_{nameof(TraitScores.Instinct)}");
                owned.Property(x => x.Presence).HasColumnName($"{nameof(GameClass.SuggestedTraits)}_{nameof(TraitScores.Presence)}");
                owned.Property(x => x.Knowledge).HasColumnName($"{nameof(GameClass.SuggestedTraits)}_{nameof(TraitScores.Knowledge)}");
            });
            entity.OwnsOne(x => x.StartingThresholds, owned =>
            {
                owned.Property(x => x.Minor).HasColumnName($"{nameof(GameClass.StartingThresholds)}_{nameof(DamageThresholds.Minor)}");
                owned.Property(x => x.Major).HasColumnName($"{nameof(GameClass.StartingThresholds)}_{nameof(DamageThresholds.Major)}");
                owned.Property(x => x.Severe).HasColumnName($"{nameof(GameClass.StartingThresholds)}_{nameof(DamageThresholds.Severe)}");
            });
            entity.HasMany(x => x.Subclasses)
                .WithOne(x => x.GameClass)
                .HasForeignKey(x => x.GameClassId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Features)
                .WithMany(x => x.GameClasses)
                .UsingEntity<Dictionary<string, object>>(
                    "GameClassFeatures",
                    right => right
                        .HasOne<Feature>()
                        .WithMany()
                        .HasForeignKey("FeatureId")
                        .OnDelete(DeleteBehavior.Cascade),
                    left => left
                        .HasOne<GameClass>()
                        .WithMany()
                        .HasForeignKey("GameClassId")
                        .OnDelete(DeleteBehavior.Cascade),
                    join =>
                    {
                        join.HasKey("GameClassId", "FeatureId");
                        join.ToTable("GameClassFeatures");
                    });
        });

        modelBuilder.Entity<Subclass>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Description).IsRequired();
            entity.HasMany(x => x.Features)
                .WithMany(x => x.Subclasses)
                .UsingEntity<Dictionary<string, object>>(
                    "SubclassFeatures",
                    right => right
                        .HasOne<Feature>()
                        .WithMany()
                        .HasForeignKey("FeatureId")
                        .OnDelete(DeleteBehavior.Cascade),
                    left => left
                        .HasOne<Subclass>()
                        .WithMany()
                        .HasForeignKey("SubclassId")
                        .OnDelete(DeleteBehavior.Cascade),
                    join =>
                    {
                        join.HasKey("SubclassId", "FeatureId");
                        join.ToTable("SubclassFeatures");
                    });
        });


        modelBuilder.Entity<Weapon>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Description).IsRequired();
            entity.OwnsOne(x => x.Damage, damage =>
            {
                damage.Property(x => x.Bonus).HasColumnName("DamageBonus");
                damage.Property(x => x.Type).HasColumnName("DamageType");
                damage.OwnsOne(x => x.Dice, dice =>
                {
                    dice.Property(x => x.NumberOfDice).HasColumnName("DamageDiceCount");
                    dice.Property(x => x.NumberOfSides).HasColumnName("DamageDiceSides");
                });
            });
            entity.HasMany(x => x.Features)
                .WithMany(x => x.Weapons)
                .UsingEntity<Dictionary<string, object>>(
                    "WeaponFeatures",
                    right => right
                        .HasOne<Feature>()
                        .WithMany()
                        .HasForeignKey("FeatureId")
                        .OnDelete(DeleteBehavior.Cascade),
                    left => left
                        .HasOne<Weapon>()
                        .WithMany()
                        .HasForeignKey("WeaponId")
                        .OnDelete(DeleteBehavior.Cascade),
                    join =>
                    {
                        join.HasKey("WeaponId", "FeatureId");
                        join.ToTable("WeaponFeatures");
                    });
        });
    }
}
