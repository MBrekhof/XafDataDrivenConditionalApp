using Microsoft.EntityFrameworkCore;
using XafDataDrivenConditionalApp.Module.BusinessObjects;
using XafDataDrivenConditionalApp.Module.ConditionalAppearance.Contracts;

namespace XafDataDrivenConditionalApp.Module.ConditionalAppearance.Services;

public sealed class EFAppearanceRuleDataProvider(
    XafDataDrivenConditionalAppEFCoreDbContext dbContext) : IAppearanceRuleDataProvider
{
    public IReadOnlyList<IAppearanceRuleData> Load(Type targetType, string context)
    {
        string typeFullName = targetType.FullName ?? targetType.Name;
        return dbContext.Set<AppearanceRuleData>()
            .AsNoTracking()
            .Where(rule => rule.IsEnabled && rule.DataTypeName.StartsWith(typeFullName)
                && (rule.Context == context || string.IsNullOrWhiteSpace(rule.Context)))
            .OrderBy(rule => rule.Priority)
            .Cast<IAppearanceRuleData>()
            .ToList();
    }
}
