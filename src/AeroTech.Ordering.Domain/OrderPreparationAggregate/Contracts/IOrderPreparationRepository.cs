namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.Contracts
{
    public interface IOrderPreparationRepository
    {
        Task<OrderPreparation?> FindInScopeAsync(long preparationId, long ownerAirlineId, long financialCustomerId, CancellationToken cancellationToken = default);

        void Add(OrderPreparation preparation);
    }
}
