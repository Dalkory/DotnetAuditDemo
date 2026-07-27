using System.Data.Entity;

namespace LegacyOrders.Web.Data
{
    public sealed class OrdersContext : DbContext
    {
        public OrdersContext() : base("Orders")
        {
        }

        public DbSet<Order> Orders { get; set; }
    }

    public sealed class Order
    {
        public int Id { get; set; }
        public string ExternalReference { get; set; }
        public decimal Amount { get; set; }
    }
}
