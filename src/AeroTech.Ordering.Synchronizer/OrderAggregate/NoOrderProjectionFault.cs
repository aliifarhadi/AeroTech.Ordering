namespace AeroTech.Ordering.Synchronizer.OrderAggregate
{
    public sealed class NoOrderProjectionFault : IOrderProjectionFaultInjector
    {
        public Task BeforeProjectionWriteAsync(long orderId, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
