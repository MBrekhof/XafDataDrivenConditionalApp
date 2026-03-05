# How to Implement Data-Driven Conditional Appearance in Your XAF App

This guide walks you through adding database-stored, runtime-editable conditional appearance rules to an existing XAF application.

## Prerequisites

- An existing XAF application (Blazor and/or WinForms) with EF Core
- The `DevExpress.ExpressApp.ConditionalAppearance` module registered in your app

Verify your Blazor `Startup.cs` (or WinForms equivalent) includes:
```csharp
builder.Modules
    .AddConditionalAppearance()
    // ...
```

## Step 1: Create the Business Object

Create `AppearanceRuleData.cs` — the entity that stores rules in your database.

```csharp
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using ModelDefaultAttribute = DevExpress.ExpressApp.Model.ModelDefaultAttribute;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

[Flags]
public enum AppearanceFontStyle
{
    Regular = 0,
    Bold = 1,
    Italic = 2,
    Underline = 4,
    Strikeout = 8
}

public enum AppearanceContext
{
    Any,
    DetailView,
    ListView
}

[DefaultClassOptions]
[DefaultProperty(nameof(Name))]
public class AppearanceRuleData
{
    [Key]
    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    public virtual int Id { get; set; }

    [Required]
    [MaxLength(256)]
    public virtual string Name { get; set; } = string.Empty;

    public virtual bool IsEnabled { get; set; } = true;

    // Stores the target type's FullName (e.g. "MyApp.BusinessObjects.Order")
    [Browsable(false)]
    [Required]
    [MaxLength(1024)]
    public virtual string DataTypeName { get; set; } = string.Empty;

    // Computed property — resolves DataTypeName to a Type for the UI dropdown
    [NotMapped]
    [TypeConverter(typeof(LocalizedClassInfoTypeConverter))]
    [ImmediatePostData]
    [XafDisplayName("Target Type")]
    public Type DataType
    {
        get
        {
            if (string.IsNullOrWhiteSpace(DataTypeName))
                return null;
            return Type.GetType(DataTypeName, throwOnError: false)
                ?? XafTypesInfo.Instance.FindTypeInfo(DataTypeName)?.Type;
        }
        set
        {
            // IMPORTANT: Store FullName, not AssemblyQualifiedName.
            // AssemblyQualifiedName includes the assembly version which changes every build.
            string newTypeName = value?.FullName ?? string.Empty;
            if (string.Equals(DataTypeName, newTypeName, StringComparison.Ordinal))
                return;
            DataTypeName = newTypeName;
            Criteria = string.Empty;
            TargetItems = string.Empty;
        }
    }

    [Browsable(false)]
    [MaxLength(128)]
    public virtual string Context { get; set; } = string.Empty;

    [NotMapped]
    [ImmediatePostData]
    [XafDisplayName("Context")]
    public AppearanceContext ContextValue
    {
        get => Enum.TryParse<AppearanceContext>(Context, out var result) ? result : AppearanceContext.Any;
        set => Context = value.ToString();
    }

    [CriteriaOptions(nameof(DataType))]
    [MaxLength(2048)]
    [FieldSize(FieldSizeAttribute.Unlimited)]
    public virtual string Criteria { get; set; } = string.Empty;

    [MaxLength(2048)]
    [XafDisplayName("Target Items")]
    [ModelDefault("ToolTip", "Property names separated by semicolons, or * for all properties")]
    public virtual string TargetItems { get; set; } = string.Empty;

    public virtual int Priority { get; set; }

    [Browsable(false)]
    [MaxLength(256)]
    public virtual string Method { get; set; } = string.Empty;

    [XafDisplayName("Visibility")]
    public virtual ViewItemVisibility? Visibility { get; set; }

    [XafDisplayName("Back Color")]
    public virtual Color? BackColor { get; set; }

    [XafDisplayName("Font Color")]
    public virtual Color? FontColor { get; set; }

    [XafDisplayName("Font Style")]
    public virtual AppearanceFontStyle? FontStyle { get; set; }
}
```

### Key design decisions

- **`Type.FullName` not `AssemblyQualifiedName`** — The assembly version changes every build. If you store `AssemblyQualifiedName`, the type lookup will break after recompilation. Use `FullName` and match with `StartsWith` in queries.
- **`[NotMapped]` computed properties** — `DataType` and `ContextValue` provide friendly UI (type picker, enum dropdown) while the persistent fields (`DataTypeName`, `Context`) store simple strings.
- **`[CriteriaOptions(nameof(DataType))]`** — Tells XAF's filter editor which type to use for building criteria expressions.
- **`Color?` properties** — XAF automatically renders color pickers for `System.Drawing.Color?` in Blazor.
- **Custom `AppearanceFontStyle`** — Avoids `System.Drawing.FontStyle` which triggers CA1416 platform warnings.

## Step 2: Configure EF Core

Add `Color?` → `int?` value converters in your DbContext since EF Core can't natively store `System.Drawing.Color`:

```csharp
public class NullableColorToInt32Converter : ValueConverter<Color?, int?>
{
    public NullableColorToInt32Converter()
        : base(
            c => c.HasValue ? c.Value.ToArgb() : null,
            v => v.HasValue ? Color.FromArgb(v.Value) : null)
    { }
}
```

In `OnModelCreating`:

```csharp
modelBuilder.Entity<AppearanceRuleData>(entity =>
{
    entity.Property(e => e.BackColor)
        .HasConversion<NullableColorToInt32Converter>()
        .HasColumnName("BackColorValue");
    entity.Property(e => e.FontColor)
        .HasConversion<NullableColorToInt32Converter>()
        .HasColumnName("FontColorValue");
    entity.Property(e => e.Visibility).HasConversion<int?>().HasColumnName("Visibility");
    entity.Property(e => e.FontStyle).HasConversion<int?>().HasColumnName("FontStyle");
});
```

Don't forget to add the `DbSet`:
```csharp
public DbSet<AppearanceRuleData> AppearanceRules { get; set; }
```

## Step 3: Create the Adapter

XAF's conditional appearance engine works with `IAppearanceRuleProperties`. Create an adapter that bridges your entity to this interface:

```csharp
using System.Drawing;
using DevExpress.Drawing;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Editors;

public sealed class AppearanceRulePropertiesAdapter : IAppearanceRuleProperties
{
    public AppearanceRulePropertiesAdapter(AppearanceRuleData rule, Type targetType)
    {
        AppearanceItemType = "ViewItem";
        Context = string.IsNullOrWhiteSpace(rule.Context) ? "Any" : rule.Context;
        Criteria = rule.Criteria;
        DeclaringType = targetType;
        Method = rule.Method ?? string.Empty;
        TargetItems = rule.TargetItems ?? "*";
        BackColor = rule.BackColor;
        FontColor = rule.FontColor;
        FontStyle = rule.FontStyle.HasValue ? (DXFontStyle)(int)rule.FontStyle.Value : null;
        Visibility = rule.Visibility;
        Enabled = null;
        Priority = rule.Priority;
    }

    public string AppearanceItemType { get; set; }
    public string Context { get; set; }
    public string Criteria { get; set; }
    public Type DeclaringType { get; }
    public string Method { get; set; }
    public string TargetItems { get; set; }
    public Color? BackColor { get; set; }
    public Color? FontColor { get; set; }
    public DXFontStyle? FontStyle { get; set; }
    public ViewItemVisibility? Visibility { get; set; }
    public bool? Enabled { get; set; }
    public int Priority { get; set; }
}
```

### Key notes

- **`AppearanceItemType = "ViewItem"`** — Targets property editors / list view cells. Other options: `"Action"`, `"LayoutItem"`.
- **`DXFontStyle`** — XAF's `IAppearance.FontStyle` uses `DevExpress.Drawing.DXFontStyle`, not `System.Drawing.FontStyle`. The int values are identical, so cast via `(DXFontStyle)(int)value`.
- **`DeclaringType`** — The target business object type. XAF uses this to scope the rule.
- **XAF auto-filters** — After `CollectAppearanceRules`, the framework automatically filters rules by `TargetItems`, `Context`, and `AppearanceItemType`. You don't need to filter manually.

## Step 4: Create the Controller

This is the core piece — a `ViewController<ObjectView>` that loads rules from the database and injects them into XAF's appearance pipeline:

```csharp
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;

public sealed class DataDrivenAppearanceController : ViewController<ObjectView>
{
    private AppearanceController appearanceController;
    private List<AppearanceRulePropertiesAdapter> cachedAdapters;

    protected override void OnActivated()
    {
        base.OnActivated();
        appearanceController = Frame.GetController<AppearanceController>();
        if (appearanceController is null)
            return;

        // Pre-load rules for this view's object type
        cachedAdapters = LoadAdaptersForCurrentView();

        if (cachedAdapters.Count > 0)
        {
            // IMPORTANT: This exact sequence is required per DevExpress docs:
            // 1. ResetRulesCache() — clear cached rules so CollectAppearanceRules fires
            // 2. Subscribe to CollectAppearanceRules
            // 3. Refresh() — trigger rule collection
            appearanceController.ResetRulesCache();
            appearanceController.CollectAppearanceRules += OnCollectAppearanceRules;
            appearanceController.Refresh();
        }
    }

    protected override void OnDeactivated()
    {
        if (appearanceController is not null)
        {
            appearanceController.CollectAppearanceRules -= OnCollectAppearanceRules;
            appearanceController = null;
        }
        cachedAdapters = null;
        base.OnDeactivated();
    }

    private void OnCollectAppearanceRules(object sender, CollectAppearanceRulesEventArgs e)
    {
        if (cachedAdapters is null || cachedAdapters.Count == 0)
            return;

        // Add all rules — XAF filters by TargetItems/Context/AppearanceItemType automatically
        foreach (var adapter in cachedAdapters)
        {
            e.AppearanceRules.Add(adapter);
        }
    }

    private List<AppearanceRulePropertiesAdapter> LoadAdaptersForCurrentView()
    {
        var objectType = View.ObjectTypeInfo?.Type;
        if (objectType is null)
            return new List<AppearanceRulePropertiesAdapter>();

        // Match by FullName prefix — handles both old AssemblyQualifiedName
        // and new FullName storage formats
        string typeFullName = objectType.FullName ?? objectType.Name;

        try
        {
            using var os = Application.CreateObjectSpace(typeof(AppearanceRuleData));
            var criteria = CriteriaOperator.Parse(
                "IsEnabled = true AND StartsWith(DataTypeName, ?)", typeFullName);
            var rules = os.GetObjects<AppearanceRuleData>(criteria);

            var adapters = new List<AppearanceRulePropertiesAdapter>();
            foreach (var rule in rules)
            {
                adapters.Add(new AppearanceRulePropertiesAdapter(rule, objectType));
            }
            return adapters;
        }
        catch (Exception)
        {
            return new List<AppearanceRulePropertiesAdapter>();
        }
    }
}
```

### Why pre-load in OnActivated?

The `CollectAppearanceRules` event fires frequently (once per UI element). Creating an `ObjectSpace` and querying the database inside the event handler would be very slow. Pre-loading and caching the rules solves this.

## Step 5: Cache Invalidation Controller

When a user edits and saves appearance rules, the changes should take effect immediately:

```csharp
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;

public sealed class ConditionalAppearanceCacheController
    : ObjectViewController<ObjectView, AppearanceRuleData>
{
    protected override void OnActivated()
    {
        base.OnActivated();
        ObjectSpace.Committed += ObjectSpaceOnCommitted;
    }

    protected override void OnDeactivated()
    {
        ObjectSpace.Committed -= ObjectSpaceOnCommitted;
        base.OnDeactivated();
    }

    private void ObjectSpaceOnCommitted(object sender, EventArgs e)
    {
        var appearanceController = Frame.GetController<AppearanceController>();
        if (appearanceController is not null)
        {
            appearanceController.ResetRulesCache();
            appearanceController.Refresh();
        }
    }
}
```

## Step 6: Target Items Dropdown (Optional but Recommended)

Make the "Target Items" field show a dropdown of available properties based on the selected target type:

```csharp
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;

public sealed class AppearanceRuleTargetItemsController
    : ObjectViewController<DetailView, AppearanceRuleData>
{
    protected override void OnActivated()
    {
        base.OnActivated();
        View.CurrentObjectChanged += (s, e) => UpdatePredefinedValues();
        ObjectSpace.ObjectChanged += (s, e) =>
        {
            if (e.PropertyName is nameof(AppearanceRuleData.DataTypeName)
                or nameof(AppearanceRuleData.DataType))
                UpdatePredefinedValues();
        };
        UpdatePredefinedValues();
    }

    private void UpdatePredefinedValues()
    {
        var rule = ViewCurrentObject as AppearanceRuleData;
        if (rule is null) return;

        var editor = View.FindItem(nameof(AppearanceRuleData.TargetItems)) as PropertyEditor;
        var modelItem = editor?.Model as IModelCommonMemberViewItem;
        if (modelItem is null) return;

        var targetType = rule.DataType;
        if (targetType is null)
        {
            modelItem.PredefinedValues = "*";
            return;
        }

        var typeInfo = XafTypesInfo.Instance.FindTypeInfo(targetType);
        if (typeInfo is null)
        {
            modelItem.PredefinedValues = "*";
            return;
        }

        var names = new List<string> { "*" };
        foreach (var member in typeInfo.Members)
        {
            if (member.IsVisible && !member.IsService)
                names.Add(member.Name);
        }
        names.Sort(1, names.Count - 1, StringComparer.OrdinalIgnoreCase);
        modelItem.PredefinedValues = string.Join(";", names);
    }
}
```

## Step 7: Register the Entity

In your module class:
```csharp
AdditionalExportedTypes.Add(typeof(AppearanceRuleData));
```

## Step 8: Layout (Optional)

Customize the detail view layout in your `.xafml` model file for a cleaner editing experience. See `Model.DesignedDiffs.xafml` in this project for a tabbed layout with Rule Definition and Appearance tabs.

## Common Pitfalls

### Type Name Mismatch
**Problem:** Rules stop working after recompilation.
**Cause:** `AssemblyQualifiedName` includes the assembly version which changes every build.
**Fix:** Store `Type.FullName` and query with `StartsWith`.

### CollectAppearanceRules Not Firing
**Problem:** The event handler is never called.
**Cause:** Rules are cached after first collection. Late subscription misses the window.
**Fix:** Always call `ResetRulesCache()` → subscribe → `Refresh()` in that exact order.

### Slow Performance
**Problem:** UI becomes sluggish with many rules.
**Cause:** Creating ObjectSpace inside `CollectAppearanceRules` (fires per UI element).
**Fix:** Pre-load rules in `OnActivated` and cache them in a list.

### FontStyle Type Mismatch
**Problem:** `CS0029: Cannot implicitly convert type 'FontStyle' to 'DXFontStyle'`.
**Cause:** XAF's `IAppearance.FontStyle` uses `DevExpress.Drawing.DXFontStyle`, not `System.Drawing.FontStyle`.
**Fix:** Cast via `(DXFontStyle)(int)value`. The enum values are identical.

### Color Properties Not Persisting
**Problem:** Colors appear to save but load as null.
**Cause:** EF Core can't natively store `System.Drawing.Color`.
**Fix:** Register `ValueConverter<Color?, int?>` in your DbContext's `OnModelCreating`.
