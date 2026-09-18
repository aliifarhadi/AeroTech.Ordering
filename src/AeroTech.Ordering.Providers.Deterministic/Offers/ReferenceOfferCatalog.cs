using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Serialization;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;
using AeroTech.Ordering.Providers.Deterministic._Shared.Persistence;
using AeroTech.Ordering.Providers.Deterministic.Offers.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ordering.Providers.Deterministic.Offers
{
    public sealed class ReferenceOfferCatalog
    {
        private readonly IDbContextFactory<DeterministicOwnerDbContext> _contextFactory;
        private readonly IClock _clock;

        public ReferenceOfferCatalog(IDbContextFactory<DeterministicOwnerDbContext> contextFactory, IClock clock)
        {
            _contextFactory = contextFactory;
            _clock = clock;
        }

        public async Task<int> PublishAsync(NormalizedCandidate candidate, CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);

            var offerId = candidate.Source.OfferId;
            var current = await context.ReferenceOffers.Where(offer => offer.OfferId == offerId && offer.IsCurrent).ToListAsync(cancellationToken);
            var revision = await context.ReferenceOffers.Where(offer => offer.OfferId == offerId).Select(offer => (int?)offer.Revision).MaxAsync(cancellationToken) ?? 0;

            current.ForEach(offer => offer.IsCurrent = false);
            await context.SaveChangesAsync(cancellationToken);

            context.ReferenceOffers.Add(new ReferenceOfferRecord
            {
                OfferId = offerId,
                Revision = revision + 1,
                OwnerBindingRef = $"{candidate.Source.OwnerBindingRef}:r{revision + 1}",
                OwnerAirlineId = candidate.SalesContext.OwnerAirlineId,
                FinancialCustomerId = candidate.SalesContext.FinancialCustomerId,
                Channel = candidate.SalesContext.Channel,
                SellingOfficeId = candidate.SalesContext.SellingOfficeId,
                CandidateJson = NormalizedCandidateJson.Write(candidate),
                IsCurrent = true,
                PublishedAt = _clock.GetDateTime()
            });

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return revision + 1;
        }

        public async Task SetAvailabilityAsync(bool available, CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var record = await context.OwnerAvailability.SingleOrDefaultAsync(item => item.Owner == ReferenceOfferProfile.Owner, cancellationToken);

            if (record is null)
                context.OwnerAvailability.Add(record = new OwnerAvailabilityRecord { Owner = ReferenceOfferProfile.Owner });

            record.IsAvailable = available;
            record.ChangedAt = _clock.GetDateTime();
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> CountReadsAsync(string? reference = null, CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            return await context.OwnerReads.CountAsync(read => read.Owner == ReferenceOfferProfile.Owner
                                                              && (reference == null || read.Reference == reference),
                cancellationToken);
        }
    }
}
