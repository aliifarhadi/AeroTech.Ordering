using System.Security.Cryptography;
using System.Text.Json;
using AeroTech.Messages;

namespace AeroTech.Ordering.Consumers.Inbox
{
    public sealed record InboxEnvelope(string SourceSystem, string EventId)
    {
        public static InboxEnvelope? From(object message)
        {
            var (sourceSystem, eventId) = message switch
            {
                BaseIntegrationEvent integrationEvent => (integrationEvent.SourceSystem, integrationEvent.EventId),
                BaseCommand command => (command.SourceSystem, command.EventId),
                _ => (null, null)
            };

            return string.IsNullOrWhiteSpace(sourceSystem) || string.IsNullOrWhiteSpace(eventId)
                ? null
                : new InboxEnvelope(sourceSystem, eventId);
        }

        public static string HashPayload<T>(T message) where T : class
            => Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(message, message.GetType())));
    }
}
