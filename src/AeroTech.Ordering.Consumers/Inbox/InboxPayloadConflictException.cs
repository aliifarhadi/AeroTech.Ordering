using AeroTech.Ordering.Persistence.Inbox;

namespace AeroTech.Ordering.Consumers.Inbox
{
    public sealed class InboxPayloadConflictException : Exception
    {
        public InboxPayloadConflictException(InboxIdentity identity, string messageType)
            : base($"Message '{messageType}' with identity ({identity.OwnerAirlineId}, {identity.SourceSystem}, {identity.EventId}, {identity.Consumer}) was already processed with a different payload.")
        {
            Identity = identity;
            MessageType = messageType;
        }

        public InboxIdentity Identity { get; }

        public string MessageType { get; }
    }
}
