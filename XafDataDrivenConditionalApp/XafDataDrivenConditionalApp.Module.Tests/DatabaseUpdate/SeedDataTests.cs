using XafDataDrivenConditionalApp.Module.DatabaseUpdate;

namespace XafDataDrivenConditionalApp.Module.Tests.DatabaseUpdate;

public sealed class SeedDataTests
{
    [Fact]
    public void CustomerOrderSeeds_HasCustomersWithOrders_AndUniqueOrderNumbers()
    {
        IReadOnlyList<CustomerSeed> seeds = SeedData.CustomerOrderSeeds;

        Assert.NotEmpty(seeds);
        Assert.All(seeds, seed => Assert.NotEmpty(seed.Orders));

        var allOrderNumbers = seeds
            .SelectMany(seed => seed.Orders)
            .Select(order => order.OrderNumber)
            .ToList();

        Assert.Equal(allOrderNumbers.Count, allOrderNumbers.Distinct(StringComparer.Ordinal).Count());
    }
}
