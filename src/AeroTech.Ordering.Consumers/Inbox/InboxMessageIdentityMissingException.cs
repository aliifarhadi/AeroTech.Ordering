namespace AeroTech.Ordering.Consumers.Inbox
{
    public sealed class InboxMessageIdentityMissingException : Exception
    {
        public InboxMessageIdentityMissingException(string messageType, string consumer)
            : base($"Message of type '{messageType}' on '{consumer}' carries no MessageId or no envelope SourceSystem/EventId and cannot be deduplicated.")
        {
            MessageType = messageType;
            Consumer = consumer;
        }

        public string MessageType { get; }

        public string Consumer { get; }
    }
}
