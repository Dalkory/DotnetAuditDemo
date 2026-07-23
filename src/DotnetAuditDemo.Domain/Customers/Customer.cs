namespace DotnetAuditDemo.Domain.Customers;

public sealed class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Email { get; set; }

    public required string FullName { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
