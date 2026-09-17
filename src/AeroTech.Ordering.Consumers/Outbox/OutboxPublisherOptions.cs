namespace AeroTech.Ordering.Consumers.Outbox
{
    public sealed class OutboxPublisherOptions
    {
        public int PublishIntervalSeconds { get; set; } = 5;

        public int BatchSize { get; set; } = 50;

        public int LeaseSeconds { get; set; } = 60;
    }
}
