using DotnetAuditDemo.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace DotnetAuditDemo.Infrastructure.Persistence;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(DemoDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();

        if (await dbContext.Customers.AnyAsync())
        {
            return;
        }

        var ada = new Customer
        {
            Email = "ada@example.test",
            FullName = "Ada Lovelace"
        };

        var grace = new Customer
        {
            Email = "grace@example.test",
            FullName = "Grace Hopper"
        };

        dbContext.Customers.AddRange(ada, grace);
        dbContext.Orders.AddRange(
            new Order
            {
                Customer = ada,
                ExternalReference = "DEMO-1001",
                Amount = 149.00m,
                Status = OrderStatus.Paid
            },
            new Order
            {
                Customer = grace,
                ExternalReference = "DEMO-1002",
                Amount = 249.00m
            });

        await dbContext.SaveChangesAsync();
    }
}
