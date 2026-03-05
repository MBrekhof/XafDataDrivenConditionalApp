using System.Drawing;
using DevExpress.Drawing;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Editors;
using XafDataDrivenConditionalApp.Module.BusinessObjects;

namespace XafDataDrivenConditionalApp.Module.ConditionalAppearance.Runtime;

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
