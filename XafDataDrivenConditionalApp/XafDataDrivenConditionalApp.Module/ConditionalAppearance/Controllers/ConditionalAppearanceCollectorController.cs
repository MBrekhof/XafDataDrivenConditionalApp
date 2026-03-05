using DevExpress.ExpressApp;
using XafDataDrivenConditionalApp.Module.ConditionalAppearance.Runtime;
using XafDataDrivenConditionalApp.Module.ConditionalAppearance.Services;

namespace XafDataDrivenConditionalApp.Module.ConditionalAppearance.Controllers;

public sealed class ConditionalAppearanceCollectorController : ViewController
{
    private readonly AppearanceRuleTranslator translator = new();
    private readonly AppearanceRuleCache cache = new();

    public IReadOnlyList<RuntimeAppearanceRule> Collect(
        Type targetType,
        string context,
        IAppearanceRuleDataProvider provider)
    {
        return cache.GetOrAdd(targetType, context, () =>
        {
            var data = provider.Load(targetType, context);
            return translator.Translate(data);
        });
    }

    public void Invalidate(Type targetType, string context) => cache.Invalidate(targetType, context);
}
