using System.Net;
using System.Net.Http.Json;

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
    public async Task AdminUsersEndpointReturnsSuccess()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/admin/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed record StatusResponse(string Status, DateTime CheckedAtUtc);
}
