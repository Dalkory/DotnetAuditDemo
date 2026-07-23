using DotnetAuditDemo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotnetAuditDemo.Tests;

public sealed class DemoApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SkipDatabaseInitialization"] = "true"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DemoDbContext>();
            services.RemoveAll<DbContextOptions<DemoDbContext>>();

            services.AddDbContext<DemoDbContext>(
                options => options.UseInMemoryDatabase("dotnet-audit-demo-tests"),
                contextLifetime: ServiceLifetime.Singleton,
                optionsLifetime: ServiceLifetime.Singleton);
        });
    }
}
