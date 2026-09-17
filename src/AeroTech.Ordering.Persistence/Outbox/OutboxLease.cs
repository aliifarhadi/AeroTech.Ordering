namespace AeroTech.Ordering.Persistence.Outbox
{
    public sealed record OutboxLease(
        long Id,
        string EventId,
        string MessageType,
        string Payload,
        string LeaseOwner,
        long LeaseVersion);
}
