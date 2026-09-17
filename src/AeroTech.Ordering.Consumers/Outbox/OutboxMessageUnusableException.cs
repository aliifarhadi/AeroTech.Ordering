namespace AeroTech.Ordering.Consumers.Outbox
{
    public sealed class OutboxMessageUnusableException : Exception
    {
        public OutboxMessageUnusableException(string message)
            : base(message)
        {
        }
    }
}
