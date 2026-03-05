using XafDataDrivenConditionalApp.Module.ConditionalAppearance.Contracts;

namespace XafDataDrivenConditionalApp.Module.ConditionalAppearance.Services;

public interface IAppearanceRuleDataProvider
{
    IReadOnlyList<IAppearanceRuleData> Load(Type targetType, string context);
}
