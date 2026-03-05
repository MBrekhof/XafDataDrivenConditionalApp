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
using XafDataDrivenConditionalApp.Module.ConditionalAppearance.Contracts;

namespace XafDataDrivenConditionalApp.Module.BusinessObjects;

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
public class AppearanceRuleData : IAppearanceRuleData
{
    public AppearanceRuleData()
    {
    }

    [Key]
    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    public virtual int Id { get; set; }

    [Required]
    [MaxLength(256)]
    [ModelDefault("AllowEdit", "True")]
    public virtual string Name { get; set; } = string.Empty;

    public virtual bool IsEnabled { get; set; } = true;

    [Browsable(false)]
    [Required]
    [MaxLength(1024)]
    public virtual string DataTypeName { get; set; } = string.Empty;

    [NotMapped]
    [TypeConverter(typeof(LocalizedClassInfoTypeConverter))]
    [ImmediatePostData]
    [XafDisplayName("Target Type")]
    public Type DataType
    {
        get
        {
            if (string.IsNullOrWhiteSpace(DataTypeName))
            {
                return null;
            }

            return Type.GetType(DataTypeName, throwOnError: false)
                ?? XafTypesInfo.Instance.FindTypeInfo(DataTypeName)?.Type;
        }
        set
        {
            // Store FullName (not AssemblyQualifiedName) to avoid version mismatch issues
            string newTypeName = value?.FullName ?? string.Empty;
            if (string.Equals(DataTypeName, newTypeName, StringComparison.Ordinal))
            {
                return;
            }

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

    [VisibleInListView(false)]
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Updated On (UTC)")]
    public virtual DateTime UpdatedOnUtc { get; set; } = DateTime.UtcNow;
}
