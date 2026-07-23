namespace DotnetAuditDemo.Application.Orders;

public interface IOrderReader
{
    Task<IReadOnlyList<OrderDto>> GetRecentAsync(CancellationToken cancellationToken);
}
