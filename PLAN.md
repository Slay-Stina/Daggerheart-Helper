# Exam Presentation — Plan 3–12 juni

**Mål:** G (Godkänt). Demo i webbläsare (PC). Publicerad på Azure Free tier.

## Aktuellt läge

- Character Creator ✅
- Character Sheet (visa + redigera, abilities, inventory) ✅
- SRD-ingestion: klasser, abilities, items, ancestries, communities, weapons, armor ✅
- SQLite + EF Core + concurrency triggers ✅
- 32 integrationstester ✅
- Designsystem (CSS-tokens, utility classes, Google Fonts) ✅
- Dashboard, karaktärslista, navigation ✅
- Item-entitet, inventory som `List<Item>`, catalog queries ✅
- 45 officiella card-bilder (ancestries, classes, communities) + 9 domain-SVG-ikoner ✅
- PR #118 (`pupil` → `master`) — redo att mergas

## Översikt dag för dag

| Dag | Datum | Aktivitet |
|-----|-------|-----------|
| 1 | 3 juni | Merga PR #118. Skapa Adversary-entitet + SRD-seeding |
| 2 | 4 juni | DM-tools: adversary-browser (filter: tier, type; sortering: difficulty) |
| 3 | 5 juni | DM-tools: SRD-flik (bläddra abilities, items, ancestries) |
| 4 | 6 juni | Bilder: card-banners i CharacterCreator (klass, subclass, heritage) |
| 5 | 7 juni | CI/CD: Azure App Service (Free F1) + GitHub Actions deploy |
| 6 | 8 juni | Testa deployment, fixa ev. problem |
| 7 | 9 juni | Rapport Sektion 5 "Lösning" — arkitektur, metodval, prioriteringar |
| 8 | 10 juni | Rapport färdigställ + bilagor (skärmdumpar, figurer) |
| 9 | 11 juni | Demo-genomgång + sista buggfixar |
| 10 | 12 juni | **Presentation** 🎯 |

---

## Dag 1 — Merga PR #118 + Adversary seeding

### Merga PR #118
- `pupil` → `master`. Innehåller Character Sheet + Item-entitet + inventory-refactor.
- Efter merge: ta bort branchen `pupil`, skapa ny feature-branch från `master`.

### Adversary
- Skapa entitet: `/Core/Entities/Adversary.cs`
  - Properties: Id, Name, Description, Difficulty (int), Tier (string — minion/boss/etc), Type (string — beast/humanoid/etc), Attack, Damage, Atk bonus, XP, Features (List<AdversaryFeature>)
- Skapa DTOs: `RawAdversaryDto.cs` (för JSON-deserialisering), `AdversaryCard.cs` (domain model)
- Skapa mappning: `SrdEntityMapper.cs` — `ToEntity(AdversaryCard)` → `Adversary`
- Uppdatera `SrdJsonLoader.cs` — ladda `adversaries.json`
- Uppdatera `SrdCatalog.cs` — lägg till `Adversaries` property
- Seeda i `Seed.cs`: alla 129 adversaries + `EnsureConcurrencyTriggersAsync`
- Test: verifiera att adversaries seedas korrekt

## Dag 2 — DM-tools: adversary-browser

### UI: `/dm-tools` 
- Flik: **Adversaries**
- Filtrering: Tier (minion, standard, boss...) + Type (beast, humanoid, elemental...)
- Sortering: difficulty (stigande/fallande)
- Sök: fritext på namn
- Visa: kort för varje adversary med difficulty-badge, type-tag, attack/damage/XP. Expandera för full text (description, features)

### Backend
- Skapa `IAdversaryCatalogQueries` + `AdversaryCatalogService`
  - `GetAllAsync()` — hämta alla
  - `SearchAsync(string query, string? tier, string? type, string? sortBy)` — filter + sök + sortering
  - `GetByIdAsync(Guid id)`
- Registrera i DI

## Dag 3 — DM-tools: SRD-flik

### Flik: **SRD Reference**
- Använd befintliga catalog queries: `IAbilityCatalogQueries`, `IItemCatalogQueries`, `IHeritageCatalogQueries`, `IClassCatalogQueries`, `IArmorCatalogQueries`, `IWeaponCatalogQueries`
- Flikar i fliken: Abilities, Items, Ancestries, Classes, Armor, Weapons
- Visa data i sökbara listor med expanderbara detaljer

## Dag 4 — Bilder som card-banners

### Assets — officiella card-banners
- Kopiera bilder från `~/Pictures/Daggerheart components/` till `Daggerheart-Helper.Shared/wwwroot/Images/`
- Struktur: `classes/`, `subclasses/`, `heritages/`

### Assets — domain-ikoner (klar ✅)
- 9 domain-SVG:er nerladdade från daggerheartsrd.com → `Images/domains/`
- CREDITS.md uppdaterad med källa
- Återstår: koppla in i UI (ability cards, filter, character creator)

### CharacterCreator
- Varje steg (Class, Subclass, Ancestry, Community) visar accordion med bild som banner
- class-bard-card.jpg → Bard, class-druid-card.jpg → Druid, etc.
- ancestries (highborne, loreborne, dwarf, elf...) → sina bilder
- Om bild saknas: fallback till nuvarande design (ingen bild)

### Mappning
- Mappa bildnamn till entity: antingen via namnkonvention eller en lookup-tabell

## Dag 5 — CI/CD: Azure App Service

### Förberedelser
- Skapa Azure App Service (Free F1) i Azure Portal eller via Azure CLI
- Välj: Runtime stack .NET 9, Linux, Region nära dig
- Notera: publish-profile eller service principal för GitHub Actions

### GitHub Actions
- Skapa `.github/workflows/deploy-web.yml`
- Steg: build → publish → deploy till Azure Web App
- Triggers: push till `master`
- Använd Azure/login + azure/webapps-deploy actions

### Viktigt
- Web-projektet (`Daggerheart-Helper.Web`) måste vara startup-projektet för deployment
- SQLite-databasen skapas automatiskt av Seed.SrdData i Program.cs
- Ingen extern databas behövs

## Dag 6 — Testa deployment

- Kör workflow, verifiera att appen syns på Azure-URL:en
- Testa Character Creator → spara → Character Sheet → redigera
- Testa DM-tools
- Fixa eventuella problem:
  - Asset-sökvägar (case-sensitive på Linux)
  - Databas-sökväg (skrivbar katalog)
  - CORS om det behövs

## Dag 7–8 — Rapport: Sektion 5 "Lösning"

### Struktur för sektion 5 (enligt MALL_Exjobb):
1. **Teknisk lösning** — beskriv arkitekturen:
   - Clean Architecture (Core, Application, Infrastructure, UI)
   - Blazor MAUI Hybrid + Web (gemensam kodbas)
   - SQLite för lokal lagring, EF Core för ORM
   - Designsystem med CSS custom properties
2. **Arbetsmetod** — Kanban, GitHub PRs som enda merge-metod, statuschecks
3. **Prioriteringar** — varför vissa features valdes bort (DM-tools mindre prioriterat, API skjuts till framtiden)
4. **Vad som är kvar** — API/Azure backend, användarautentisering, molnsynk, DM-verktyg är grundläggande

### Bilagor
- Figma-mockups (om de finns)
- Skärmdumpar av appen
- Systemkarta/arkitekturdiagram

## Dag 9 — Demo-genomgång

- Torrkör presentationen
- Webb-app på Azure → öppen på datorskärmen
- Gå igenom: Dashboard → Create Character → visa alla steg → Character Sheet → redigera → DM-tools
- Kolla att allt funkar i incognito-fönster (rensad cache)

---

## GitHub Issues att skapa

| Issue | Title | Labels | Milestone |
|-------|-------|--------|-----------|
| 1 | Adversary entity + SRD seeding | Core, SRD | M5 |
| 2 | DM-tools: adversary-browser med filter/sort | UI, DM-tools | M5 |
| 3 | DM-tools: SRD Reference-flik | UI, DM-tools | M5 |
| 4 | Card-banners med officiella bilder i CharacterCreator | UI | M5 |
| 5 | CI/CD: Azure App Service deployment | Infrastructure | M5 |
| 6 | Rapport: Sektion 5 "Lösning" | Documentation | M5 |
| 7 | Demo-polering: sista buggfixar | UI | M5 |

---

## Risker

- **Azure Free tier slut** — F1 har 60 min CPU/dygn, räcker för demo men inte tung användning
- **Bildlicenser** — Darrington Press bilder får användas i community content, men kolla licensen
- **Bygget tar lång tid** — `dotnet workload restore` för MAUI kan vara segt på CI
- **SQLite på Azure** — web-appen kör Linux, filbaserad SQLite funkar men data försvinner vid omstart (acceptabelt för demo)
