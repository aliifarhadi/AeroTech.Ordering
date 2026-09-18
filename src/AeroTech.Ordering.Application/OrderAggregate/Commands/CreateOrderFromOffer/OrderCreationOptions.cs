namespace AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer
{
    public sealed class OrderCreationOptions
    {
        public const string SectionName = "OrderCreation";

        public int MaxReferenceAttempts { get; set; } = 3;
    }
}
