using DotnetAuditDemo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DotnetAuditDemo.Api.BackgroundJobs;

public sealed class InvoiceReminderWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<InvoiceReminderWorker> logger) : BackgroundService
{
    private static readonly Action<ILogger, int, Exception?> LogPendingInvoices =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(3001, nameof(InvoiceReminderWorker)),
            "{PendingCount} invoices still require payment");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DemoDbContext>();

            // Deliberate sync-over-async problem inside a background worker.
            var pendingCount = dbContext.Orders.CountAsync(
                order => order.Status == Domain.Customers.OrderStatus.Pending,
                stoppingToken).Result;

            LogPendingInvoices(logger, pendingCount, null);
        }
    }
}
