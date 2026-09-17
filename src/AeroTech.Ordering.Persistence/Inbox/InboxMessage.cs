namespace AeroTech.Ordering.Persistence.Inbox
{
    public sealed class InboxMessage
    {
        public long OwnerAirlineId { get; set; }

        public string SourceSystem { get; set; } = null!;

        public string EventId { get; set; } = null!;

        public string Consumer { get; set; } = null!;

        public string MessageType { get; set; } = null!;

        public string PayloadHash { get; set; } = null!;

        public DateTimeOffset ReceivedOn { get; set; }
    }
}
