using XafDataDrivenConditionalApp.Module.BusinessObjects;
using System.Collections.Specialized;

namespace XafDataDrivenConditionalApp.Module.Tests.BusinessObjects;

public sealed class CustomerOrderTests
{
    [Fact]
    public void Customer_InitializesOrdersCollection()
    {
        var customer = new Customer();

        Assert.NotNull(customer.Orders);
        Assert.Empty(customer.Orders);
    }

    [Fact]
    public void Customer_OrdersCollection_ImplementsINotifyCollectionChanged()
    {
        var customer = new Customer();

        Assert.IsAssignableFrom<INotifyCollectionChanged>(customer.Orders);
    }

    [Fact]
    public void Order_CanReferenceCustomer()
    {
        var customer = new Customer { Name = "Contoso" };
        var order = new Order
        {
            OrderNumber = "SO-1001",
            Customer = customer
        };

        Assert.Same(customer, order.Customer);
    }

    [Fact]
    public void DbContext_ExposesCustomersAndOrdersDbSets()
    {
        var customerSetProperty = typeof(XafDataDrivenConditionalAppEFCoreDbContext).GetProperty(nameof(XafDataDrivenConditionalAppEFCoreDbContext.Customers));
        var orderSetProperty = typeof(XafDataDrivenConditionalAppEFCoreDbContext).GetProperty(nameof(XafDataDrivenConditionalAppEFCoreDbContext.Orders));

        Assert.NotNull(customerSetProperty);
        Assert.NotNull(orderSetProperty);
    }
}
