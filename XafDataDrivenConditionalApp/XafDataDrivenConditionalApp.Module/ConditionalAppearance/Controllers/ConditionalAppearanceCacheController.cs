using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using XafDataDrivenConditionalApp.Module.BusinessObjects;

namespace XafDataDrivenConditionalApp.Module.ConditionalAppearance.Controllers;

public sealed class ConditionalAppearanceCacheController : ObjectViewController<ObjectView, AppearanceRuleData>
{
    internal static event EventHandler RulesCommitted;

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

        RulesCommitted?.Invoke(this, EventArgs.Empty);
    }
}
