namespace AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer
{
    public interface ICreateOrderFromOfferService
    {
        Task<CreateOrderFromOfferResult> ExecuteAsync(CreateOrderFromOfferArgs args, CancellationToken cancellationToken = default);
    }
}
