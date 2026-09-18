namespace AeroTech.Ordering.Query.OrderAggregate.Models
{
    public sealed class OrderDetailsReadModel
    {
        public long OrderId { get; set; }

        public long OwnerAirlineId { get; set; }

        public long FinancialCustomerId { get; set; }

        public string OrderReference { get; set; } = default!;

        public long OrderRevision { get; set; }

        public int CommercialVersion { get; set; }

        public int ProjectionSchemaVersion { get; set; }

        public string DetailsJson { get; set; } = default!;

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset ProjectedAt { get; set; }
    }
}
