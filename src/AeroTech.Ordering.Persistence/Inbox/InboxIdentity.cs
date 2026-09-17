namespace AeroTech.Ordering.Persistence.Inbox
{
    public sealed record InboxIdentity(long OwnerAirlineId, string SourceSystem, string EventId, string Consumer);
}
