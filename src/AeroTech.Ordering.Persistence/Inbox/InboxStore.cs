using AeroTech.Framework.Core.ServiceContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AeroTech.Ordering.Persistence.Inbox
{
    public sealed class InboxStore
    {
        private readonly OrderingDbContext _dbContext;
        private readonly IClock _clock;

        private EntityEntry<InboxMessage>? _enlisted;

        public InboxStore(OrderingDbContext dbContext, IClock clock)
        {
            _dbContext = dbContext;
            _clock = clock;
        }

        public Task<string?> FindPayloadHashAsync(InboxIdentity identity, CancellationToken cancellationToken = default)
            => _dbContext.Set<InboxMessage>()
                .AsNoTracking()
                .Where(message => message.OwnerAirlineId == identity.OwnerAirlineId
                                  && message.SourceSystem == identity.SourceSystem
                                  && message.EventId == identity.EventId
                                  && message.Consumer == identity.Consumer)
                .Select(message => message.PayloadHash)
                .SingleOrDefaultAsync(cancellationToken);

        public void EnlistProcessed(InboxIdentity identity, string messageType, string payloadHash)
            => _enlisted = _dbContext.Set<InboxMessage>().Add(
                new InboxMessage
                {
                    OwnerAirlineId = identity.OwnerAirlineId,
                    SourceSystem = identity.SourceSystem,
                    EventId = identity.EventId,
                    Consumer = identity.Consumer,
                    MessageType = messageType,
                    PayloadHash = payloadHash,
                    ReceivedOn = _clock.GetDateTime()
                });

        public async Task PersistProcessedAsync(CancellationToken cancellationToken = default)
        {
            if (_enlisted is { State: EntityState.Added })
                await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
