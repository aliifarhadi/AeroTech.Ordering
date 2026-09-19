using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Aegis.Enums;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Messages.Shared.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Application;
using AeroTech.Ordering.Application._Shared.Authorization;
using AeroTech.Ordering.Domain._Shared.Contracts;
using AeroTech.Ordering.Domain.OrderAggregate.Contracts;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Contracts;
using AeroTech.Ordering.Domain.Tests._Shared;
using AeroTech.Ordering.Persistence;
using AeroTech.Ordering.Providers;
using AeroTech.Ordering.Providers.AirOffer;
using AeroTech.Ordering.Providers.Deterministic;
using AeroTech.Ordering.Providers.Deterministic._Shared.Persistence;
using AeroTech.Ordering.Providers.Deterministic.Offers;
using AeroTech.Ordering.Query;
using AeroTech.Ordering.ReferenceData;
using AeroTech.Ordering.ReferenceData.Persistence;
using AeroTech.Ordering.ReferenceData.ReadModels;
using AeroTech.Ordering.Synchronizer;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AeroTech.Ordering.Persistence.Tests._Shared
{
    public sealed class S1Harness : IAsyncDisposable
    {
        public const long OwnerAirlineId = 1;
        public const string DigestKey = "s1-test-digest-key";

        public const long CustomerId = 100;
        public const long OtherCustomerId = 200;
        public const long AgencyCustomerId = 300;
        public const long SuspendedCustomerId = 400;
        public const long TravelAgencyId = 7;
        public const long AirlineOfficeId = 10;
        public const long AgencyOfficeId = 9;
        public const long AirlineUserId = 1;
        public const long AgencyUserId = 2;
        public const long PartnerApiAccessProfileId = 3;

        private readonly ServiceProvider _provider;

        private S1Harness(ServiceProvider provider, TestClock clock, TestCallerContext caller)
        {
            _provider = provider;
            Clock = clock;
            Caller = caller;
        }

        public TestClock Clock { get; }

        public TestCallerContext Caller { get; }

        public IServiceProvider Services => _provider;

        public ReferenceOfferCatalog Catalog => _provider.GetRequiredService<ReferenceOfferCatalog>();

        public static async Task<S1Harness> StartAsync(
            OrderingDatabaseFixture fixture,
            Action<IServiceCollection>? configure = null,
            TestClock? clock = null,
            bool useReferenceOffers = true,
            string? offerBaseUrl = null)
        {
            var testClock = clock ?? new TestClock();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:CommandDbContext"] = fixture.ConnectionString,
                    [$"ConnectionStrings:{DeterministicOwnerDbContext.ConnectionStringName}"] = fixture.OwnerConnectionString,
                    ["Idempotency:DigestKey"] = DigestKey,
                    ["Offer:BaseUrl"] = offerBaseUrl ?? "http://airoffer.test/service/",
                    ["Offer:RequestTimeout"] = "2",
                    ["Offer:RetryCount"] = "0",
                    ["Offer:RetryInterval"] = "0"
                })
                .Build();

            var caller = new TestCallerContext().UseBackoffice(AirlineOfficeId, AirlineUserId);

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<ICallerContext>(caller);
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<IClock>(testClock);
            services.AddSingleton<IIdGenerator>(SequentialIdGenerator.Unique());
            services.AddSingleton<IIdentityService, OrderingDatabaseFixture.NullIdentityService>();
            services.AddSingleton<TimeProvider>(TimeProvider.System);

            services
                .AddPersistence(configuration)
                .AddProviders(configuration);

            if (useReferenceOffers)
                services.AddDeterministicProviders(configuration);

            services
                .AddQuery(configuration)
                .AddSynchronizer()
                .AddApplication(configuration)
                .AddReferenceData(configuration);

            services.AddSingleton<IAcceptanceProfilePolicy>(new TestAcceptanceProfilePolicy(ReferenceOfferProfile.ProfileId, AirOfferProfile.LiveCandidateSandbox));

            configure?.Invoke(services);

            var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
            var harness = new S1Harness(provider, testClock, caller);

            await harness.EnsureHomeOperatorAsync();
            await harness.EnsureCustomersAsync();

            if (useReferenceOffers)
                await provider.GetRequiredService<DeterministicEffectStore>().EnsureSchemaAsync();

            return harness;
        }

        public async Task<TResult> SendAsync<TResult>(IRequest<TResult> request)
        {
            await using var scope = _provider.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<IMediator>().Send(request);
        }

        public async Task<TResult> InScopeAsync<TResult>(Func<IServiceProvider, Task<TResult>> action)
        {
            await using var scope = _provider.CreateAsyncScope();
            return await action(scope.ServiceProvider);
        }

        public static Domain._Shared.ValueObjects.AuthorizedSalesScope Scope(long customerId = CustomerId, long actorId = AirlineUserId)
            => new(
                OwnerAirlineId,
                customerId,
                new SalesContextSnapshot(
                    SalesChannel.BackOffice,
                    BusinessContextType.Airline,
                    OwnerAirlineId,
                    SellingOfficeKind.AirlineOffice,
                    AirlineOfficeId),
                BuyerSnapshot.NotSupplied,
                CallerScopeKey.ForSale(CallerScopeKey.Backoffice, customerId, AirlineOfficeId, CallerScopeKey.None),
                new InitiatingActorSnapshot(BusinessContextType.Airline, actorId));

        public static Domain._Shared.ValueObjects.AuthorizedSalesScope ServiceScope(long customerId = CustomerId)
            => new(
                OwnerAirlineId,
                customerId,
                SalesContextSnapshot.SellerNotSupplied(SalesChannel.System, null, null),
                BuyerSnapshot.NotSupplied,
                CallerScopeKey.ForSale(CallerScopeKey.Service, customerId, null, CallerScopeKey.None),
                new InitiatingActorSnapshot(BusinessContextType.Service, null));

        public static Domain._Shared.ValueObjects.AuthorizedSalesScope OtaPanelScope(long customerId = AgencyCustomerId)
            => new(
                OwnerAirlineId,
                customerId,
                new SalesContextSnapshot(
                    SalesChannel.AgencyPanel,
                    BusinessContextType.TravelAgency,
                    TravelAgencyId,
                    SellingOfficeKind.TravelAgencyOffice,
                    AgencyOfficeId),
                BuyerSnapshot.NotSupplied,
                CallerScopeKey.ForSale(CallerScopeKey.OtaPanel, customerId, AgencyOfficeId, CallerScopeKey.Agency(TravelAgencyId)),
                new InitiatingActorSnapshot(BusinessContextType.TravelAgency, AgencyUserId));

        public static Domain._Shared.ValueObjects.AuthorizedSalesScope OtaScope(long customerId = OtherCustomerId)
            => new(
                OwnerAirlineId,
                customerId,
                SalesContextSnapshot.SellerNotSupplied(SalesChannel.PartnerAPI, null, null),
                BuyerSnapshot.NotSupplied,
                CallerScopeKey.ForSale(CallerScopeKey.Ota, customerId, null, CallerScopeKey.Partner(PartnerApiAccessProfileId)),
                new InitiatingActorSnapshot(BusinessContextType.PartnerApi, PartnerApiAccessProfileId));

        public async ValueTask DisposeAsync() => await _provider.DisposeAsync();

        private async Task EnsureHomeOperatorAsync()
        {
            await using var scope = _provider.CreateAsyncScope();
            var reference = scope.ServiceProvider.GetRequiredService<ReferenceDbContext>();

            if (await reference.OperatorSettings.AnyAsync(settings => settings.ScopeKey == OperatorScopeKey.HomeOperator))
                return;

            reference.OperatorSettings.Add(new OperatorSettingsReadModel
            {
                Id = 1,
                ScopeKey = OperatorScopeKey.HomeOperator,
                HomeAirlineId = OwnerAirlineId,
                LastUpdateTime = DateTimeOffset.UtcNow
            });

            await reference.SaveChangesAsync();
        }

        private async Task EnsureCustomersAsync()
        {
            await using var scope = _provider.CreateAsyncScope();
            var reference = scope.ServiceProvider.GetRequiredService<ReferenceDbContext>();

            if (await reference.Customers.AnyAsync())
                return;

            reference.Customers.AddRange(
                Customer(CustomerId, CustomerType.Individual, null, CustomerStatus.Active),
                Customer(OtherCustomerId, CustomerType.Individual, null, CustomerStatus.Active),
                Customer(AgencyCustomerId, CustomerType.TravelAgency, TravelAgencyId, CustomerStatus.Active),
                Customer(SuspendedCustomerId, CustomerType.Individual, null, CustomerStatus.Suspended));

            await reference.SaveChangesAsync();
        }

        private static CustomerReadModel Customer(long id, CustomerType type, long? travelAgencyId, CustomerStatus status) => new()
        {
            Id = id,
            CustomerNumber = $"C{id}",
            Type = type,
            TravelAgencyId = travelAgencyId,
            SubjectId = id,
            SubjectName = $"Customer {id}",
            Status = status,
            PreferredCurrencyId = null,
            LastUpdateTime = DateTimeOffset.UtcNow
        };

        private sealed class TestAcceptanceProfilePolicy : IAcceptanceProfilePolicy
        {
            private readonly HashSet<string> _permitted;

            public TestAcceptanceProfilePolicy(params string[] permitted) => _permitted = new HashSet<string>(permitted, StringComparer.Ordinal);

            public string EnvironmentClass => "S1Test";

            public bool Permits(string acceptanceProfile) => _permitted.Contains(acceptanceProfile);
        }
    }
}
