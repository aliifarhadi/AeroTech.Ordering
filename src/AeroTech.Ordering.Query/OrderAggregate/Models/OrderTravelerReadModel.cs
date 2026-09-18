namespace AeroTech.Ordering.Query.OrderAggregate.Models
{
    public sealed class OrderTravelerReadModel
    {
        public long Id { get; set; }

        public long OrderId { get; set; }

        public string SourceTravellerRef { get; set; } = default!;

        public string ClientTravelerRef { get; set; } = default!;

        public string PassengerTypeCode { get; set; } = default!;

        public long? InfantParentTravelerId { get; set; }
    }
}
