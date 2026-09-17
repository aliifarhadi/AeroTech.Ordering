using AeroTech.Framework.Core.ServiceContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace AeroTech.Ordering.Providers.Deterministic._Shared.Persistence
{
    public sealed class DeterministicEffectStore
    {
        private readonly IDbContextFactory<DeterministicOwnerDbContext> _contextFactory;
        private readonly IClock _clock;

        public DeterministicEffectStore(IDbContextFactory<DeterministicOwnerDbContext> contextFactory, IClock clock)
        {
            _contextFactory = contextFactory;
            _clock = clock;
        }

        public async Task EnsureSchemaAsync(CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var creator = context.GetService<IRelationalDatabaseCreator>();

            if (!await creator.ExistsAsync(cancellationToken))
                await creator.CreateAsync(cancellationToken);

            if (!await creator.HasTablesAsync(cancellationToken))
                await creator.CreateTablesAsync(cancellationToken);
        }

        public async Task<DeterministicOwnerEffect?> FindAsync(string owner, string effectKey, CancellationToken cancellationToken = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            return await context.OwnerEffects
                .AsNoTracking()
                .SingleOrDefaultAsync(effect => effect.Owner == owner && effect.EffectKey == effectKey, cancellationToken);
        }

        public async Task<DeterministicOwnerEffect> RecordAsync(
            string owner,
            string effectKey,
            string requestHash,
            string resultPayload,
            CancellationToken cancellationToken = default)
        {
            var existing = await FindAsync(owner, effectKey, cancellationToken);
            if (existing is not null)
                return SameRequestOrConflict(existing, requestHash);

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var effect = new DeterministicOwnerEffect
            {
                Owner = owner,
                EffectKey = effectKey,
                RequestHash = requestHash,
                ResultPayload = resultPayload,
                RecordedOn = _clock.GetDateTime()
            };

            context.OwnerEffects.Add(effect);

            try
            {
                await context.SaveChangesAsync(cancellationToken);
                return effect;
            }
            catch (DbUpdateException)
            {
                var winner = await FindAsync(owner, effectKey, cancellationToken);
                if (winner is null)
                    throw;

                return SameRequestOrConflict(winner, requestHash);
            }
        }

        private static DeterministicOwnerEffect SameRequestOrConflict(DeterministicOwnerEffect existing, string requestHash)
            => string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal)
                ? existing
                : throw new DeterministicEffectConflictException(existing.Owner, existing.EffectKey);
    }
}
