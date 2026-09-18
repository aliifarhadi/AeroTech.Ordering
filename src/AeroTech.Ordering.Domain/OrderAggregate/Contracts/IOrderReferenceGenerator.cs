namespace AeroTech.Ordering.Domain.OrderAggregate.Contracts
{
    public interface IOrderReferenceGenerator
    {
        Task<string> NextAsync(long ownerAirlineId, CancellationToken cancellationToken = default);
    }
}
