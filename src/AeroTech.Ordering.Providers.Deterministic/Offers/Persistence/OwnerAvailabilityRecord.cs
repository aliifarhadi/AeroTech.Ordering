namespace AeroTech.Ordering.Providers.Deterministic.Offers.Persistence
{
    public sealed class OwnerAvailabilityRecord
    {
        public string Owner { get; set; } = null!;

        public bool IsAvailable { get; set; }

        public DateTimeOffset ChangedAt { get; set; }
    }
}
