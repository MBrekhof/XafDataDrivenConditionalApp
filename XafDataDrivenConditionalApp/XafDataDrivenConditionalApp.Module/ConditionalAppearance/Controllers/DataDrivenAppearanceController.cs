using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using XafDataDrivenConditionalApp.Module.BusinessObjects;
using XafDataDrivenConditionalApp.Module.ConditionalAppearance.Runtime;

namespace XafDataDrivenConditionalApp.Module.ConditionalAppearance.Controllers;

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

        cachedAdapters = LoadAdaptersForCurrentView();

        appearanceController.ResetRulesCache();
        appearanceController.CollectAppearanceRules += OnCollectAppearanceRules;
        appearanceController.Refresh();

        ConditionalAppearanceCacheController.RulesCommitted += OnRulesCommitted;
    }

    protected override void OnDeactivated()
    {
        ConditionalAppearanceCacheController.RulesCommitted -= OnRulesCommitted;
        if (appearanceController is not null)
        {
            appearanceController.CollectAppearanceRules -= OnCollectAppearanceRules;
            appearanceController = null;
        }
        cachedAdapters = null;
        base.OnDeactivated();
    }

    private void OnRulesCommitted(object sender, EventArgs e)
    {
        if (appearanceController is null)
            return;

        cachedAdapters = LoadAdaptersForCurrentView();
        appearanceController.ResetRulesCache();
        appearanceController.Refresh();
    }

    private void OnCollectAppearanceRules(object sender, CollectAppearanceRulesEventArgs e)
    {
        if (cachedAdapters is null || cachedAdapters.Count == 0)
            return;

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

        // Match by FullName prefix since stored DataTypeName may include
        // AssemblyQualifiedName with volatile version info
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
