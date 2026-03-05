using System.Collections.Concurrent;
using XafDataDrivenConditionalApp.Module.ConditionalAppearance.Runtime;

namespace XafDataDrivenConditionalApp.Module.ConditionalAppearance.Services;

public sealed class AppearanceRuleCache
{
    private readonly ConcurrentDictionary<(Type TargetType, string Context), IReadOnlyList<RuntimeAppearanceRule>> cache = new();

    public IReadOnlyList<RuntimeAppearanceRule> GetOrAdd(
        Type targetType,
        string context,
        Func<IReadOnlyList<RuntimeAppearanceRule>> valueFactory)
    {
        return cache.GetOrAdd((targetType, context), _ => valueFactory());
    }

    public void Invalidate(Type targetType, string context)
    {
        cache.TryRemove((targetType, context), out _);
    }

    public void InvalidateAll()
    {
        cache.Clear();
    }
}
