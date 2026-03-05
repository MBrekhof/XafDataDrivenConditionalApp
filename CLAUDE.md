# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Is

A DevExpress XAF application that implements **data-driven conditional appearance rules** — appearance rules (colors, visibility, font styles) stored in the database instead of being hard-coded as attributes. Users can create/edit appearance rules at runtime through the UI, and they take effect immediately.

## Build & Run

```bash
# Build the entire solution
dotnet build XafDataDrivenConditionalApp.slnx

# Run the Blazor Server app
dotnet run --project XafDataDrivenConditionalApp/XafDataDrivenConditionalApp.Blazor.Server

# Run tests (xUnit)
dotnet test XafDataDrivenConditionalApp/XafDataDrivenConditionalApp.Module.Tests

# Run a single test
dotnet test XafDataDrivenConditionalApp/XafDataDrivenConditionalApp.Module.Tests --filter "FullyQualifiedName~TestMethodName"
```

## Solution Structure

```
XafDataDrivenConditionalApp.slnx          # .NET 8, DevExpress 25.2, SQL Server (LocalDB)
├── XafDataDrivenConditionalApp.Module     # Shared platform-agnostic module (net8.0)
├── XafDataDrivenConditionalApp.Blazor.Server  # Blazor Server host (net8.0)
├── XafDataDrivenConditionalApp.Win        # WinForms host (net8.0-windows)
└── XafDataDrivenConditionalApp.Module.Tests   # xUnit tests (net10.0)
```

## Architecture: Data-Driven Conditional Appearance

The core pipeline in `Module/ConditionalAppearance/`:

1. **`AppearanceRuleData`** (BusinessObjects) — EF Core entity storing rules. Uses `FullName` for `DataTypeName`, nullable `Color?` for colors (via `NullableColorToInt32Converter`), custom `AppearanceFontStyle` flags enum.

2. **`AppearanceRulePropertiesAdapter`** (Runtime) — Bridges `AppearanceRuleData` → XAF's `IAppearanceRuleProperties` interface. Maps `AppearanceFontStyle` → `DXFontStyle`, passes through `Color?`, `ViewItemVisibility?`.

3. **`DataDrivenAppearanceController`** (Controllers) — The core controller. Pre-loads rules in `OnActivated`, hooks into `AppearanceController.CollectAppearanceRules`, injects adapters. Uses `StartsWith(DataTypeName, fullName)` to match types (avoids assembly version mismatch).

4. **`ConditionalAppearanceCacheController`** (Controllers) — Resets `AppearanceController` rule cache on commit so changes take effect immediately.

5. **`AppearanceRuleTargetItemsController`** (Controllers) — Dynamically populates `PredefinedValues` on the TargetItems field based on selected DataType.

### Key Patterns

- **Always store `Type.FullName`, never `AssemblyQualifiedName`** — assembly version changes every build, breaking exact matches
- **Query with `StartsWith`** to handle both old AQN and new FullName formats in existing data
- **Pre-load rules in OnActivated** — don't create ObjectSpace inside `CollectAppearanceRules` (fires per UI element)
- **`ResetRulesCache()` → subscribe → `Refresh()`** — required sequence per DX docs

## Business Objects

- **`Customer`** — Name, Email, has many Orders
- **`Order`** — OrderNumber, OrderDate, TotalAmount, belongs to Customer
- **`AppearanceRuleData`** — Runtime-configurable appearance rules

## Database

- EF Core with `XafDataDrivenConditionalAppEFCoreDbContext`
- SQL Server LocalDB (`appsettings.json` → `ConnectionStrings:ConnectionString`)
- Seed data in `DatabaseUpdate/SeedData.cs`
- Uses XAF conventions: deferred deletion, optimistic locking, `ChangingAndChangedNotifications`

## XAF Modules

The app module requires: `SystemModule`, `ConditionalAppearanceModule`, `ValidationModule`
