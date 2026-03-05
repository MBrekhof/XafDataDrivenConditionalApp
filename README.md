# XAF Data-Driven Conditional Appearance

A DevExpress XAF application demonstrating **data-driven conditional appearance rules** — appearance rules stored in the database that users can create and edit at runtime through the UI, without recompiling the application.

## What It Does

Instead of hard-coding `[Appearance]` attributes on your business classes, this project lets end users define appearance rules through a built-in admin screen:

- **Target Type** — which business object the rule applies to (e.g. `Order`, `Customer`)
- **Target Items** — which properties to affect (dropdown populated from the target type, or `*` for all)
- **Criteria** — XAF criteria expression (e.g. `[TotalAmount] < 0`) using the built-in filter editor
- **Context** — where the rule applies: Any, DetailView, or ListView
- **Appearance** — back color, font color, font style (bold/italic/underline/strikeout), visibility
- **Priority** — rule ordering when multiple rules affect the same element

Rules take effect immediately after saving — no restart required.

## Screenshots

The rule editor has two tabs:

**Rule Definition** — configure what the rule targets and when it activates:
- Name, enabled flag, priority, context
- Target type (dropdown of all business objects)
- Target items (dropdown of properties from the selected type)
- Criteria (XAF filter editor with visual builder)

**Appearance** — configure the visual effect:
- Back color and font color (color pickers)
- Font style (Bold, Italic, Underline, Strikeout)
- Visibility (Show, Hide, ShowEmptySpace)

## Tech Stack

- .NET 8 / C#
- DevExpress XAF 25.2 (Blazor Server + WinForms)
- EF Core with SQL Server LocalDB
- DevExpress Conditional Appearance Module

## Getting Started

### Prerequisites

- .NET 8 SDK
- DevExpress Universal Subscription (v25.2)
- SQL Server LocalDB (included with Visual Studio)

### Run

```bash
# Build
dotnet build XafDataDrivenConditionalApp.slnx

# Run the Blazor Server app
dotnet run --project XafDataDrivenConditionalApp/XafDataDrivenConditionalApp.Blazor.Server

# Run tests
dotnet test XafDataDrivenConditionalApp/XafDataDrivenConditionalApp.Module.Tests
```

The database is created automatically on first run using the connection string in `appsettings.json`.

## Project Structure

```
XafDataDrivenConditionalApp.Module/
├── BusinessObjects/
│   ├── AppearanceRuleData.cs          # The rule entity (DB-stored appearance rules)
│   ├── Customer.cs                     # Sample business object
│   ├── Order.cs                        # Sample business object
│   └── XafDataDrivenConditionalAppDbContext.cs  # EF Core DbContext with value converters
├── ConditionalAppearance/
│   ├── Contracts/
│   │   └── IAppearanceRuleData.cs      # Rule data interface
│   ├── Runtime/
│   │   ├── AppearanceRulePropertiesAdapter.cs  # Bridges DB entity → XAF IAppearanceRuleProperties
│   │   └── RuntimeAppearanceRule.cs    # Immutable runtime rule record
│   ├── Services/
│   │   ├── AppearanceRuleCache.cs      # ConcurrentDictionary-based rule cache
│   │   ├── AppearanceRuleTranslator.cs # Converts entities to runtime rules
│   │   ├── EFAppearanceRuleDataProvider.cs  # Loads rules from EF Core
│   │   └── IAppearanceRuleDataProvider.cs   # Provider interface
│   └── Controllers/
│       ├── DataDrivenAppearanceController.cs           # Core: hooks rules into XAF's appearance pipeline
│       ├── ConditionalAppearanceCacheController.cs     # Invalidates cache on rule save
│       └── ConditionalAppearanceCollectorController.cs # Rule collection orchestrator
├── Controllers/
│   └── AppearanceRuleTargetItemsController.cs  # Dynamic dropdown for Target Items
├── DatabaseUpdate/
│   ├── SeedData.cs                     # Sample data definitions
│   └── Updater.cs                      # Database migration/seed
└── Model.DesignedDiffs.xafml           # UI layout for the rule editor
```

## How It Works

1. **`AppearanceRuleData`** stores rules in the database with all appearance properties (colors as ARGB ints, font style as flags enum, visibility as int)

2. **`DataDrivenAppearanceController`** activates on every `ObjectView`, loads matching rules from the DB, and hooks into XAF's `AppearanceController.CollectAppearanceRules` event

3. **`AppearanceRulePropertiesAdapter`** wraps each `AppearanceRuleData` as an `IAppearanceRuleProperties` — the interface XAF's conditional appearance engine understands

4. XAF's built-in engine then evaluates the criteria against each object, filters by context/target items, and applies the visual changes

5. **`ConditionalAppearanceCacheController`** watches for rule saves and resets the appearance cache so changes take effect immediately

## License

This is a demonstration project. Use it as a reference for implementing data-driven conditional appearance in your own XAF applications. See [HOW_TO_IMPLEMENT.md](HOW_TO_IMPLEMENT.md) for a step-by-step guide.
