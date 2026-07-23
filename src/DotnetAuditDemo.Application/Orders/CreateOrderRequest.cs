namespace DotnetAuditDemo.Application.Orders;

public sealed record CreateOrderRequest(
    string CustomerEmail,
    string CustomerName,
    string ExternalReference,
    decimal Amount);
