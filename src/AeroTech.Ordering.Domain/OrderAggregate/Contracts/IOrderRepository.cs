namespace AeroTech.Ordering.Domain.OrderAggregate.Contracts
{
    public interface IOrderRepository
    {
        void Add(Order order);

        Task<Order?> LoadSnapshotAsync(long orderId, long ownerAirlineId, CancellationToken cancellationToken = default);
    }
}
