namespace XafDataDrivenConditionalApp.Module.ConditionalAppearance.Contracts;

public interface IAppearanceRuleData
{
    string Name { get; }
    bool IsEnabled { get; }
    string DataTypeName { get; }
    string Context { get; }
    string Criteria { get; }
    string TargetItems { get; }
    int Priority { get; }
}
