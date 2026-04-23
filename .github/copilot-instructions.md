# Copilot Instructions for Daggerheart-Helper

## Quick Start

### Build & Test Commands

**Full solution build:**
```bash
dotnet build
```

**Run tests (if any exist):**
```bash
dotnet test --no-build --verbosity normal
```

**Run the web application:**
```bash
dotnet run --project Daggerheart-Helper.Web
# Web app will be at https://localhost:5001
```

**Run the MAUI desktop/mobile app:**
```bash
dotnet run --project Daggerheart-Helper -c Debug
```

**Restore dependencies and workloads:**
```bash
dotnet workload restore
dotnet restore
```

**Database migrations (when schema changes are made):**
```bash
dotnet ef migrations add <MigrationName> --project Infrastructure
dotnet ef database update --project Infrastructure
```

### Project Structure

**Solution Layout:**
- **Core** - Domain models, enums, and value objects for Daggerheart game rules
  - `Entities/` - Character, GameClass, Subclass, Ability, Feature, Weapon, Armor, FeatureEffect
  - `Enums/` - AbilityType, Ancestry, Burden, Community, DamageType, Domain, etc.
  - `Value Objects/` - Damage, DamageThresholds, Dice, ResourcePool, TraitScores
- **Infrastructure** - Data persistence and dependency injection
  - `Persistence/DaggerheartDbContext.cs` - EF Core DbContext with all entity configurations
  - `DependencyInjection.cs` - Service registration extension (`AddDaggerheartPersistence`)
- **Daggerheart-Helper.Shared** - Razor components and shared UI for web/MAUI
  - `Pages/` - Razor page components
  - `Layout/` - Layout components
  - Uses Fluent UI components
- **Daggerheart-Helper.Web** - ASP.NET Core Blazor web application (net9.0)
  - Entry point: `Program.cs` configures services and middleware
  - Uses Fluent UI for web components
  - Requires `DefaultConnection` connection string in appsettings
- **Daggerheart-Helper** - MAUI cross-platform application (net9.0-{android,ios,maccatalyst,windows,tizen})
  - `MauiProgram.cs` - MAUI app configuration
  - `Platforms/` - Platform-specific entry points
- **External** - Git submodule for daggerheart-srd data source

## Key Conventions

### Entity & Value Object Patterns
- **Value Objects** (in `Value Objects/` folder) are immutable types with no identity, used for composed properties (Damage, TraitScores, ResourcePool)
- **Entities** (in `Entities/` folder) have identity (Guid Id) and represent core domain concepts
- **Enums** stored as strings in the database (see `DaggerheartDbContext.ConfigureConventions`)

### Entity Framework Configuration
- All entity configurations are in `DaggerheartDbContext.OnModelCreating()`
- Owned types (Value Objects) are configured inline using `OwnsOne()`
- Many-to-many relationships use explicit join entities (e.g., `ArmorFeatures`, `GameClassFeatures`)
- Resource pools use `OwnsOne()` pattern with renamed columns (HitPointsCurrent, HitPointsMax, etc.)
- Check constraints are used to validate domain rules (e.g., Trait type required when target is Trait)

### Dependency Injection
- Service registration is centralized in `Infrastructure/DependencyInjection.cs` via the `AddDaggerheartPersistence()` extension
- Call this from `Program.cs` and pass the connection string
- The DbContext is configured for SQLite

### Database Configuration
- Connection string key: `"DefaultConnection"` in appsettings.json
- Default SQLite database file location: `daggerheart.db` (set in `DaggerheartDbContextFactory`)
- Nullable reference types are enabled (`<Nullable>enable</Nullable>`)

### Code Style
- Implicit usings are enabled (`<ImplicitUsings>enable</ImplicitUsings>`)
- File-scoped namespaces are used (namespace without braces)
- Target framework is .NET 9.0 (with platform-specific variants for MAUI)

### Submodule Management
- External data is in `External/daggerheart-srd/` as a git submodule
- Always run `git submodule update --init --recursive` after cloning

## Architecture Notes

This is a multi-project solution that separates concerns:
- **Domain Layer (Core)** contains game rules with no external dependencies
- **Persistence Layer (Infrastructure)** handles EF Core, DbContext, and service registration
- **Presentation Layer** is split across:
  - **Daggerheart-Helper.Web** for server-rendered Blazor web UI
  - **Daggerheart-Helper** for MAUI native/cross-platform desktop/mobile apps
  - **Daggerheart-Helper.Shared** for shared Razor components and UI logic used by both

The solution supports rendering the same Razor components on web and MAUI through shared assemblies registered in `Program.cs` with `AddAdditionalAssemblies()`.

## Testing & CI

The GitHub Actions workflow (`.github/workflows/.NET`) runs on pull requests:
- Checks out code
- Sets up .NET SDK (version from global.json)
- Restores workloads and dependencies
- Builds the solution
- Runs tests if any `*Tests.csproj` files are found

When adding new test projects, follow the naming convention `*Tests.csproj` so they are automatically discovered by CI.
