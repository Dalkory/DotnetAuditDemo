namespace DotnetAuditDemo.Application.Orders;

public sealed record OrderDto(
    Guid Id,
    string ExternalReference,
    decimal Amount,
    string Status,
    string CustomerEmail,
    DateTime CreatedAtUtc);
