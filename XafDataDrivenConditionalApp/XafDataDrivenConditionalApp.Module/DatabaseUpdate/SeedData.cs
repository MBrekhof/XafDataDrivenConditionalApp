namespace XafDataDrivenConditionalApp.Module.DatabaseUpdate;

public sealed record OrderSeed(string OrderNumber, DateTime OrderDate, decimal TotalAmount);

public sealed record CustomerSeed(string Name, string Email, IReadOnlyList<OrderSeed> Orders);

public static class SeedData
{
    public static IReadOnlyList<CustomerSeed> CustomerOrderSeeds { get; } =
    [
        new CustomerSeed(
            "Contoso Ltd",
            "sales@contoso.example",
            [
                new OrderSeed("SO-1001", new DateTime(2026, 1, 15), 1250.00m),
                new OrderSeed("SO-1002", new DateTime(2026, 2, 3), 980.50m)
            ]),
        new CustomerSeed(
            "Northwind Traders",
            "orders@northwind.example",
            [
                new OrderSeed("SO-2001", new DateTime(2026, 1, 28), 2100.00m),
                new OrderSeed("SO-2002", new DateTime(2026, 2, 19), 455.75m)
            ])
    ];
}
