using DotnetAuditDemo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotnetAuditDemo.Tests;

public sealed class DemoApiFactory : WebApplicationFactory<Program>
{
    public DemoApiFactory()
    {
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__Orders",
            "Host=unused;Database=unused;Username=unused;Password=unused");
        Environment.SetEnvironmentVariable(
            "Jwt__SigningKey",
            "test-only-signing-key-at-least-32-characters");
        Environment.SetEnvironmentVariable("SkipDatabaseInitialization", "true");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DemoDbContext>();
            services.RemoveAll<DbContextOptions<DemoDbContext>>();

            services.AddDbContext<DemoDbContext>(
                options => options.UseInMemoryDatabase("dotnet-audit-demo-tests"));
        });
    }
}
