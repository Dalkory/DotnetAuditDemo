namespace DotnetAuditDemo.Domain.Customers;

public sealed class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    public required string ExternalReference { get; set; }

    public decimal Amount { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public enum OrderStatus
{
    Pending,
    Paid,
    Cancelled
}
