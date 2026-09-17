using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Ordering.Domain.Tests._Shared;
using AeroTech.Ordering.Persistence.Outbox;
using AeroTech.Ordering.Persistence.Tests._Shared;
using AeroTech.Ordering.Providers.Deterministic;
using AeroTech.Ordering.Providers.Deterministic._Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.Deterministic
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class DeterministicEffectStoreTests
    {
        private const string Owner = "Inventory";

        private readonly OrderingDatabaseFixture _fixture;

        public DeterministicEffectStoreTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Owner_effect_survives_an_ordering_rollback_and_a_restart()
        {
            var effectKey = $"hold-{Guid.NewGuid():N}";
            var orderingEventId = $"owner-{Guid.NewGuid():N}";

            await using (var provider = BuildProvider())
            {
                var store = provider.GetRequiredService<DeterministicEffectStore>();
                await store.EnsureSchemaAsync();

                await using var ordering = _fixture.NewCommandContext();
                await using var transaction = await ordering.Database.BeginTransactionAsync();

                ordering.OutboxMessages.Add(new OutboxMessage
                {
                    EventId = orderingEventId,
                    MessageType = "ordering-local-work",
                    Payload = "{}",
                    OccurredOn = DateTimeOffset.UtcNow
                });
                await ordering.SaveChangesAsync();

                await store.RecordAsync(Owner, effectKey, "hash-1", "{\"held\":true}");

                await transaction.RollbackAsync();
            }

            await using (var ordering = _fixture.NewCommandContext())
                Assert.False(await ordering.OutboxMessages.AnyAsync(message => message.EventId == orderingEventId));

            await using var restarted = BuildProvider();
            var effect = await restarted.GetRequiredService<DeterministicEffectStore>().FindAsync(Owner, effectKey);

            Assert.NotNull(effect);
            Assert.Equal("hash-1", effect.RequestHash);
            Assert.Equal("{\"held\":true}", effect.ResultPayload);
        }

        [Fact]
        public async Task Same_key_and_hash_replays_and_a_different_hash_conflicts()
        {
            var effectKey = $"hold-{Guid.NewGuid():N}";

            await using var provider = BuildProvider();
            var store = provider.GetRequiredService<DeterministicEffectStore>();
            await store.EnsureSchemaAsync();

            var first = await store.RecordAsync(Owner, effectKey, "hash-1", "first");
            var replay = await store.RecordAsync(Owner, effectKey, "hash-1", "second");

            Assert.Equal(first.Id, replay.Id);
            Assert.Equal("first", replay.ResultPayload);

            var conflict = await Assert.ThrowsAsync<DeterministicEffectConflictException>(
                () => store.RecordAsync(Owner, effectKey, "hash-2", "third"));

            Assert.Equal(effectKey, conflict.EffectKey);
        }

        [Fact]
        public void Owner_database_is_not_the_ordering_database()
            => Assert.NotEqual(_fixture.DatabaseName, _fixture.OwnerDatabaseName);

        private ServiceProvider BuildProvider()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [$"ConnectionStrings:{DeterministicOwnerDbContext.ConnectionStringName}"] = _fixture.OwnerConnectionString
                })
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IClock>(new TestClock());
            services.AddDeterministicProviders(configuration);

            return services.BuildServiceProvider(true);
        }
    }
}
