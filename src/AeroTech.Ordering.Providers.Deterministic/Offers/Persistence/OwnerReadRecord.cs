namespace AeroTech.Ordering.Providers.Deterministic.Offers.Persistence
{
    public sealed class OwnerReadRecord
    {
        public long Id { get; set; }

        public string Owner { get; set; } = null!;

        public string Operation { get; set; } = null!;

        public string Reference { get; set; } = null!;

        public string Outcome { get; set; } = null!;

        public DateTimeOffset ObservedAt { get; set; }
    }
}
