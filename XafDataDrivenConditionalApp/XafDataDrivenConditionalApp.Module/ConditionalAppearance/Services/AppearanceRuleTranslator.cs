using XafDataDrivenConditionalApp.Module.ConditionalAppearance.Contracts;
using XafDataDrivenConditionalApp.Module.ConditionalAppearance.Runtime;

namespace XafDataDrivenConditionalApp.Module.ConditionalAppearance.Services;

public sealed class AppearanceRuleTranslator
{
    public IReadOnlyList<RuntimeAppearanceRule> Translate(IEnumerable<IAppearanceRuleData> ruleData)
    {
        var translated = new List<RuntimeAppearanceRule>();
        foreach (IAppearanceRuleData item in ruleData.Where(data => data.IsEnabled))
        {
            Type ruleType = Type.GetType(item.DataTypeName, throwOnError: false);
            if (ruleType is null || string.IsNullOrWhiteSpace(item.Criteria))
            {
                continue;
            }

            translated.Add(new RuntimeAppearanceRule(
                item.Name,
                ruleType,
                item.Context,
                item.Criteria,
                item.TargetItems,
                item.Priority));
        }

        return translated
            .OrderBy(rule => rule.Priority)
            .ToArray();
    }
}
