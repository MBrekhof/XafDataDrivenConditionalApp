namespace XafDataDrivenConditionalApp.Module.ConditionalAppearance.Runtime;

public sealed record RuntimeAppearanceRule(
    string Name,
    Type TargetType,
    string Context,
    string Criteria,
    string TargetItems,
    int Priority);
