using AeroTech.Ordering.Domain._Shared.Contracts;
using AeroTech.Ordering.Persistence.Inbox;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AeroTech.Ordering.Consumers.Inbox
{
    public sealed class InboxConsumeFilter<T> : IFilter<ConsumeContext<T>> where T : class
    {
        private readonly InboxStore _inboxStore;
        private readonly IHomeOperatorProvider _homeOperatorProvider;
        private readonly ILogger<InboxConsumeFilter<T>> _logger;

        public InboxConsumeFilter(
            InboxStore inboxStore,
            IHomeOperatorProvider homeOperatorProvider,
            ILogger<InboxConsumeFilter<T>> logger)
        {
            _inboxStore = inboxStore;
            _homeOperatorProvider = homeOperatorProvider;
            _logger = logger;
        }

        public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            var consumer = context.ReceiveContext.InputAddress?.AbsolutePath ?? typeof(T).Name;
            var messageType = typeof(T).FullName ?? typeof(T).Name;

            if (context.MessageId is null || InboxEnvelope.From(context.Message) is not { } envelope)
                throw new InboxMessageIdentityMissingException(messageType, consumer);

            var ownerAirlineId = await _homeOperatorProvider.GetOwnerAirlineIdAsync(context.CancellationToken);
            var identity = new InboxIdentity(ownerAirlineId, envelope.SourceSystem, envelope.EventId, consumer);
            var payloadHash = InboxEnvelope.HashPayload(context.Message);

            if (await IsProcessedAsync(identity, payloadHash, messageType, context.CancellationToken))
            {
                _logger.LogInformation(
                    "Skipped duplicate message {SourceSystem}/{EventId} of type {MessageType} on {Consumer}.",
                    identity.SourceSystem,
                    identity.EventId,
                    messageType,
                    consumer);

                return;
            }

            _inboxStore.EnlistProcessed(identity, messageType, payloadHash);

            try
            {
                await next.Send(context);
                await _inboxStore.PersistProcessedAsync(context.CancellationToken);
            }
            catch (DbUpdateException exception) when (InboxDuplicateClassifier.IsDuplicateMarker(exception))
            {
                if (await IsProcessedAsync(identity, payloadHash, messageType, context.CancellationToken))
                {
                    _logger.LogInformation(
                        "Discarded concurrent duplicate message {SourceSystem}/{EventId} of type {MessageType} on {Consumer}.",
                        identity.SourceSystem,
                        identity.EventId,
                        messageType,
                        consumer);

                    return;
                }

                throw;
            }
        }

        public void Probe(ProbeContext context) => context.CreateFilterScope("inbox");

        private async Task<bool> IsProcessedAsync(
            InboxIdentity identity,
            string payloadHash,
            string messageType,
            CancellationToken cancellationToken)
        {
            var processedHash = await _inboxStore.FindPayloadHashAsync(identity, cancellationToken);

            if (processedHash is null)
                return false;

            if (!string.Equals(processedHash, payloadHash, StringComparison.Ordinal))
                throw new InboxPayloadConflictException(identity, messageType);

            return true;
        }
    }
}
