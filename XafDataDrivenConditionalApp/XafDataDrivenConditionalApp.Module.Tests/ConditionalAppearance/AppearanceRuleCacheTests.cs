using XafDataDrivenConditionalApp.Module.ConditionalAppearance.Runtime;
using XafDataDrivenConditionalApp.Module.ConditionalAppearance.Services;

namespace XafDataDrivenConditionalApp.Module.Tests.ConditionalAppearance;

public sealed class AppearanceRuleCacheTests
{
    [Fact]
    public void GetOrAdd_SameKey_UsesFactoryOnlyOnce()
    {
        var cache = new AppearanceRuleCache();
        var calls = 0;
        var keyType = typeof(string);

        IReadOnlyList<RuntimeAppearanceRule> first = cache.GetOrAdd(keyType, "DetailView", () =>
        {
            calls++;
            return new[]
            {
                new RuntimeAppearanceRule("R1", keyType, "DetailView", "[Length] > 0", "Length", 1)
            };
        });
        IReadOnlyList<RuntimeAppearanceRule> second = cache.GetOrAdd(keyType, "DetailView", () =>
        {
            calls++;
            return Array.Empty<RuntimeAppearanceRule>();
        });

        Assert.Equal(1, calls);
        Assert.Single(first);
        Assert.Single(second);
    }

    [Fact]
    public void Invalidate_SpecificKey_RefreshesCachedRules()
    {
        var cache = new AppearanceRuleCache();
        var calls = 0;
        var keyType = typeof(int);

        _ = cache.GetOrAdd(keyType, "ListView", () =>
        {
            calls++;
            return new[]
            {
                new RuntimeAppearanceRule("Old", keyType, "ListView", "[Value] >= 0", "Value", 0)
            };
        });
        cache.Invalidate(keyType, "ListView");
        IReadOnlyList<RuntimeAppearanceRule> refreshed = cache.GetOrAdd(keyType, "ListView", () =>
        {
            calls++;
            return new[]
            {
                new RuntimeAppearanceRule("New", keyType, "ListView", "[Value] >= 10", "Value", 0)
            };
        });

        Assert.Equal(2, calls);
        Assert.Equal("New", refreshed[0].Name);
    }
}
