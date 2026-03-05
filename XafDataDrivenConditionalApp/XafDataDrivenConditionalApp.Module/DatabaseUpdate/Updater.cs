using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Updating;
using XafDataDrivenConditionalApp.Module.BusinessObjects;

namespace XafDataDrivenConditionalApp.Module.DatabaseUpdate
{
    // For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Updating.ModuleUpdater
    public class Updater : ModuleUpdater
    {
        public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
            base(objectSpace, currentDBVersion)
        {
        }

        public override void UpdateDatabaseAfterUpdateSchema()
        {
            base.UpdateDatabaseAfterUpdateSchema();
            SeedCustomersAndOrders();
        }

        public override void UpdateDatabaseBeforeUpdateSchema()
        {
            base.UpdateDatabaseBeforeUpdateSchema();
        }

        private void SeedCustomersAndOrders()
        {
            var hasChanges = false;

            foreach (CustomerSeed customerSeed in SeedData.CustomerOrderSeeds)
            {
                Customer customer = ObjectSpace.FirstOrDefault<Customer>(c => c.Name == customerSeed.Name)
                    ?? CreateCustomer(customerSeed, ref hasChanges);

                if (!string.Equals(customer.Email, customerSeed.Email, StringComparison.Ordinal))
                {
                    customer.Email = customerSeed.Email;
                    hasChanges = true;
                }

                foreach (OrderSeed orderSeed in customerSeed.Orders)
                {
                    Order order = ObjectSpace.FirstOrDefault<Order>(o => o.OrderNumber == orderSeed.OrderNumber)
                        ?? CreateOrder(orderSeed, ref hasChanges);

                    if (!ReferenceEquals(order.Customer, customer))
                    {
                        order.Customer = customer;
                        hasChanges = true;
                    }

                    if (order.OrderDate != orderSeed.OrderDate)
                    {
                        order.OrderDate = orderSeed.OrderDate;
                        hasChanges = true;
                    }

                    if (order.TotalAmount != orderSeed.TotalAmount)
                    {
                        order.TotalAmount = orderSeed.TotalAmount;
                        hasChanges = true;
                    }
                }
            }

            if (hasChanges)
            {
                ObjectSpace.CommitChanges();
            }
        }

        private Customer CreateCustomer(CustomerSeed seed, ref bool hasChanges)
        {
            var customer = ObjectSpace.CreateObject<Customer>();
            customer.Name = seed.Name;
            customer.Email = seed.Email;
            hasChanges = true;
            return customer;
        }

        private Order CreateOrder(OrderSeed seed, ref bool hasChanges)
        {
            var order = ObjectSpace.CreateObject<Order>();
            order.OrderNumber = seed.OrderNumber;
            order.OrderDate = seed.OrderDate;
            order.TotalAmount = seed.TotalAmount;
            hasChanges = true;
            return order;
        }
    }
}
