using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using XafDataDrivenConditionalApp.Module.BusinessObjects;

namespace XafDataDrivenConditionalApp.Module.Controllers;

public sealed class AppearanceRuleTargetItemsController
    : ObjectViewController<DetailView, AppearanceRuleData>
{
    protected override void OnActivated()
    {
        base.OnActivated();
        View.CurrentObjectChanged += View_CurrentObjectChanged;
        ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;
        UpdateTargetItemsPredefinedValues();
    }

    protected override void OnDeactivated()
    {
        View.CurrentObjectChanged -= View_CurrentObjectChanged;
        ObjectSpace.ObjectChanged -= ObjectSpace_ObjectChanged;
        base.OnDeactivated();
    }

    private void View_CurrentObjectChanged(object sender, EventArgs e)
    {
        UpdateTargetItemsPredefinedValues();
    }

    private void ObjectSpace_ObjectChanged(object sender, ObjectChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AppearanceRuleData.DataTypeName)
            || e.PropertyName == nameof(AppearanceRuleData.DataType))
        {
            UpdateTargetItemsPredefinedValues();
        }
    }

    private void UpdateTargetItemsPredefinedValues()
    {
        var rule = ViewCurrentObject as AppearanceRuleData;
        if (rule is null)
            return;

        var editor = View.FindItem(nameof(AppearanceRuleData.TargetItems)) as PropertyEditor;
        if (editor is null)
            return;

        var modelItem = editor.Model as IModelCommonMemberViewItem;
        if (modelItem is null)
            return;

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

        var propertyNames = new List<string> { "*" };
        foreach (var member in typeInfo.Members)
        {
            if (member.IsVisible && !member.IsService)
            {
                propertyNames.Add(member.Name);
            }
        }

        propertyNames.Sort(1, propertyNames.Count - 1, StringComparer.OrdinalIgnoreCase);

        modelItem.PredefinedValues = string.Join(";", propertyNames);
    }
}
