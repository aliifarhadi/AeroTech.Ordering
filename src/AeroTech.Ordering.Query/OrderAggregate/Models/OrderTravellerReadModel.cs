using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Query.OrderAggregate.Models
{
    public sealed class OrderTravellerReadModel
    {
        public long Id { get; set; }

        public long OrderId { get; set; }

        public string SourceTravellerRef { get; set; } = default!;

        public string ClientTravellerRef { get; set; } = default!;

        public PassengerTypeCode PassengerTypeCode { get; set; }

        public long? InfantParentTravellerId { get; set; }
    }
}
