namespace AeroTech.Ordering.Synchronizer.OrderAggregate
{
    public interface IOrderProjectionFaultInjector
    {
        Task BeforeProjectionWriteAsync(long orderId, CancellationToken cancellationToken);
    }
}
