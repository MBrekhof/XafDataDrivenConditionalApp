using XafDataDrivenConditionalApp.Module.ConditionalAppearance.Contracts;
using XafDataDrivenConditionalApp.Module.ConditionalAppearance.Runtime;
using XafDataDrivenConditionalApp.Module.ConditionalAppearance.Services;

namespace XafDataDrivenConditionalApp.Module.Tests.ConditionalAppearance;

public sealed class AppearanceRuleTranslatorTests
{
    [Fact]
    public void Translate_EnabledRuleWithKnownTypeAndCriteria_ReturnsRuntimeRule()
    {
        var translator = new AppearanceRuleTranslator();
        var rules = new IAppearanceRuleData[]
        {
            new TestRuleData
            {
                Name = "Active Customer Name ReadOnly",
                DataTypeName = typeof(string).AssemblyQualifiedName!,
                Context = "DetailView",
                Criteria = "[Length] > 0",
                TargetItems = "Length",
                Priority = 1,
                IsEnabled = true
            }
        };

        IReadOnlyList<RuntimeAppearanceRule> runtimeRules = translator.Translate(rules);

        Assert.Single(runtimeRules);
        Assert.Equal("Active Customer Name ReadOnly", runtimeRules[0].Name);
    }

    [Fact]
    public void Translate_DisabledRule_IgnoresRule()
    {
        var translator = new AppearanceRuleTranslator();
        var rules = new IAppearanceRuleData[]
        {
            new TestRuleData
            {
                Name = "Disabled Rule",
                DataTypeName = typeof(string).AssemblyQualifiedName!,
                Criteria = "[Length] > 0",
                IsEnabled = false
            }
        };

        IReadOnlyList<RuntimeAppearanceRule> runtimeRules = translator.Translate(rules);

        Assert.Empty(runtimeRules);
    }

    [Fact]
    public void Translate_UnknownType_IgnoresRule()
    {
        var translator = new AppearanceRuleTranslator();
        var rules = new IAppearanceRuleData[]
        {
            new TestRuleData
            {
                Name = "Unknown Type",
                DataTypeName = "Missing.Type, Missing.Assembly",
                Criteria = "[Any] > 0",
                IsEnabled = true
            }
        };

        IReadOnlyList<RuntimeAppearanceRule> runtimeRules = translator.Translate(rules);

        Assert.Empty(runtimeRules);
    }

    private sealed class TestRuleData : IAppearanceRuleData
    {
        public string Name { get; init; } = string.Empty;
        public bool IsEnabled { get; init; }
        public string DataTypeName { get; init; } = string.Empty;
        public string Context { get; init; } = string.Empty;
        public string Criteria { get; init; } = string.Empty;
        public string TargetItems { get; init; } = string.Empty;
        public int Priority { get; init; }
    }
}
