namespace AeroTech.Ordering.Persistence.Outbox
{
    public sealed class OutboxMessage
    {
        public long Id { get; set; }

        public string EventId { get; set; } = null!;

        public string MessageType { get; set; } = null!;

        public string Payload { get; set; } = null!;

        public DateTimeOffset OccurredOn { get; set; }

        public DateTimeOffset? ProcessedOn { get; set; }

        public int AttemptCount { get; set; }

        public string? LastError { get; set; }

        public string? LeaseOwner { get; set; }

        public DateTimeOffset? LeaseExpiresOn { get; set; }

        public long LeaseVersion { get; set; }
    }
}
