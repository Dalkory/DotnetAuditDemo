using DotnetAuditDemo.Api.Clients;
using DotnetAuditDemo.Application.Orders;
using DotnetAuditDemo.Domain.Customers;
using DotnetAuditDemo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetAuditDemo.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(
    DemoDbContext dbContext,
    ExchangeRateClient exchangeRateClient,
    ILogger<OrdersController> logger) : ControllerBase
{
    private static readonly Action<ILogger, string, string, Exception?> LogOrderCreation =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(1001, nameof(Create)),
            "Creating order {ExternalReference} for {CustomerEmail}");

    private static readonly Action<ILogger, Exception?> LogOrderCreationFailure =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(1002, "OrderCreationFailed"),
            "Order creation failed");

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        // Cancellation is intentionally left for a later implementation batch (F-006).
        var response = await dbContext.Orders
            .AsNoTracking()
            .OrderByDescending(order => order.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(order => new OrderDto(
                order.Id,
                order.ExternalReference,
                order.Amount,
                order.Status.ToString(),
                order.Customer.Email,
                order.CreatedAtUtc))
            .ToListAsync();

        return Ok(response);
    }

    [HttpGet("by-reference/{externalReference}")]
    public async Task<ActionResult<OrderDto>> GetByReference(string externalReference)
    {
        // ExternalReference is frequently queried but has no database index.
        var order = await dbContext.Orders
            .Include(item => item.Customer)
            .SingleOrDefaultAsync(item => item.ExternalReference == externalReference);

        if (order is null)
        {
            return NotFound();
        }

        return Ok(new OrderDto(
            order.Id,
            order.ExternalReference,
            order.Amount,
            order.Status.ToString(),
            order.Customer.Email,
            order.CreatedAtUtc));
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest request)
    {
        try
        {
            // Deliberate PII logging and no input validation.
            LogOrderCreation(
                logger,
                request.ExternalReference,
                request.CustomerEmail,
                null);

            // Deliberate sync-over-async in a request path.
            var eurRate = exchangeRateClient.GetEurRateAsync().Result;

            var customer = await dbContext.Customers.SingleOrDefaultAsync(
                item => item.Email == request.CustomerEmail);

            if (customer is null)
            {
                customer = new Customer
                {
                    Email = request.CustomerEmail,
                    FullName = request.CustomerName
                };
                dbContext.Customers.Add(customer);
            }

            var order = new Order
            {
                Customer = customer,
                ExternalReference = request.ExternalReference,
                Amount = request.Amount * eurRate
            };

            dbContext.Orders.Add(order);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetByReference),
                new { externalReference = order.ExternalReference },
                new OrderDto(
                    order.Id,
                    order.ExternalReference,
                    order.Amount,
                    order.Status.ToString(),
                    customer.Email,
                    order.CreatedAtUtc));
        }
        catch (Exception exception)
        {
            // Deliberate broad catch that hides failure categories.
            LogOrderCreationFailure(logger, exception);
            return StatusCode(StatusCodes.Status500InternalServerError, "Order creation failed");
        }
    }
}
