using System.Net;
using System.Net.Http.Json;
using DotnetAuditDemo.Domain.Customers;
using DotnetAuditDemo.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetAuditDemo.Tests;

public sealed class ApiHappyPathTests(DemoApiFactory factory) : IClassFixture<DemoApiFactory>
{
    [Fact]
    public async Task StatusEndpointReturnsRunningStatus()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/status");
        var payload = await response.Content.ReadFromJsonAsync<StatusResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("running", payload?.Status);
    }

    [Fact]
    public async Task AdminUsersEndpointRejectsAnonymousRequest()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/admin/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OrdersEndpointCapsRequestedPageSize()
    {
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DemoDbContext>();
            var customer = new Customer
            {
                Email = "pagination@example.test",
                FullName = "Pagination Test"
            };

            dbContext.Orders.AddRange(
                Enumerable.Range(1, 120).Select(index => new Order
                {
                    Customer = customer,
                    ExternalReference = $"PAGE-{index:000}",
                    Amount = index
                }));

            await dbContext.SaveChangesAsync();
        }

        using var client = factory.CreateClient();
        var response = await client.GetFromJsonAsync<List<OrderResponse>>(
            "/api/orders?page=1&pageSize=999");

        Assert.Equal(100, response?.Count);
    }

    private sealed record StatusResponse(string Status, DateTime CheckedAtUtc);

    private sealed record OrderResponse(
        Guid Id,
        string ExternalReference,
        decimal Amount,
        string Status,
        string CustomerEmail,
        DateTime CreatedAtUtc);
}
