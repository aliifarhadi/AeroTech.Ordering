using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.CommandReceiptAggregate;
using AeroTech.Ordering.Domain.OrderAggregate;
using AeroTech.Ordering.Domain.OrderPreparationAggregate;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;
using AeroTech.Ordering.Domain.Tests._Shared;
using AeroTech.Ordering.Persistence.Tests._Shared;
using AeroTech.Ordering.Query.OrderAggregate.Models;
using AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrderById.Backoffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.S1
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class CreateOrderFromOfferTests
    {
        private readonly OrderingDatabaseFixture _fixture;

        public CreateOrderFromOfferTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task SC_S1_002_create_commits_one_item_one_service_total_120_and_no_fulfillment_effects()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);

            var created = await harness.SendAsync(S1Commands.Backoffice(offerId));

            var order = await LoadOrderAsync(harness, created.OrderId);
            Assert.Single(order.Items);
            Assert.Single(order.Services);
            Assert.Equal(2, order.PricingLines.Count);
            Assert.Equal(120.00m, order.CustomerTotal.Amount);
            Assert.Equal(1, order.CommercialVersion);
            Assert.Equal(1, order.FinancialSequence);
            Assert.Equal(1, order.OrderRevision);
            Assert.Single(order.FundingObligations);
            Assert.Equal("120", created.GrandTotal);
            Assert.Equal(CommercialSummary.Active, created.Status);
            Assert.Matches("^[23456789ABCDEFGHJKLMNPQRSTUVWXYZ]{8}$", created.OrderReference);

            await using var scope = harness.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();

            Assert.Equal(created.OrderId, (await db.Set<OrderPreparation>().AsNoTracking().SingleAsync(item => item.SourceOfferId == offerId)).ConsumedByOrderId);
            Assert.Equal(CommandReceiptStatus.Completed, (await db.Set<CommandReceipt>().AsNoTracking().SingleAsync(receipt => receipt.OrderId == created.OrderId)).Status);

            var outbox = await db.OutboxMessages.AsNoTracking().SingleAsync(message => message.StreamId == created.OrderId);
            Assert.Equal("Order", outbox.StreamKind);
            Assert.Equal(1, outbox.EventOrdinal);
            Assert.Contains("OrderCreated", outbox.MessageType);

            var order_dto = await harness.SendAsync(new BackofficeGetOrderByIdQuery(created.OrderId));
            Assert.Equal(created.OrderReference, order_dto.OrderReference);
            Assert.Equal("120", order_dto.GrandTotal.Amount);
            Assert.Equal(offerId, order_dto.OfferId);
            Assert.Single(order_dto.Items);
            Assert.Single(Assert.Single(order_dto.Items).Services);
        }

        [Fact]
        public async Task SC_S1_001_owner_reprice_after_the_sale_does_not_change_the_created_order()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);

            var created = await harness.SendAsync(S1Commands.Backoffice(offerId));

            await harness.Catalog.PublishAsync(new CandidateBuilder(harness.Clock.GetDateTime(), S1Harness.Scope())
                .Offer(offerId)
                .Traveler("PAX-A")
                .Segment("SEG-A")
                .AirService("SERVICE-A", "PAX-A", "SEG-A")
                .Package("ITEM-A", "SERVICE-A")
                .Line("PRICE-FARE", "ITEM-A", PricingComponentType.Fare, 130.00m, "SERVICE-A")
                .Line("PRICE-TAX", "ITEM-A", PricingComponentType.Tax, 20.00m, "SERVICE-A")
                .Build());

            var repriced = await harness.SendAsync(S1Commands.Backoffice(offerId));

            Assert.Equal(120.00m, (await LoadOrderAsync(harness, created.OrderId)).CustomerTotal.Amount);
            Assert.Equal(150.00m, (await LoadOrderAsync(harness, repriced.OrderId)).CustomerTotal.Amount);
            Assert.NotEqual(created.OrderId, repriced.OrderId);
        }

        [Fact]
        public async Task SC_S1_003_replay_after_owner_outage_returns_the_same_order_without_owner_access()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);
            var command = S1Commands.Backoffice(offerId);
            var created = await harness.SendAsync(command);

            await harness.Catalog.SetAvailabilityAsync(false);
            var readsDuringOutage = await harness.Catalog.CountReadsAsync();

            try
            {
                var replayed = await harness.SendAsync(command);

                Assert.Equal(created.OrderId, replayed.OrderId);
                Assert.Equal(created.OrderReference, replayed.OrderReference);
                Assert.Equal(created.GrandTotal, replayed.GrandTotal);
                Assert.Equal(readsDuringOutage, await harness.Catalog.CountReadsAsync());
                Assert.Equal(1, await CountAsync<Order>(harness, order => order.Id == created.OrderId));

                var unavailable = await Assert.ThrowsAsync<BusinessException>(() => harness.SendAsync(S1Commands.Backoffice(offerId)));
                Assert.Equal(20275, unavailable.Code);
            }
            finally
            {
                await harness.Catalog.SetAvailabilityAsync(true);
            }
        }

        [Fact]
        public async Task SC_S1_004_same_key_with_a_different_request_is_a_conflict_and_changes_nothing()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var first = await PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);
            var second = await PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);
            var key = S1Commands.NewKey("shared");

            var created = await harness.SendAsync(S1Commands.Backoffice(first, key: key));
            var conflict = await Assert.ThrowsAsync<BusinessException>(() => harness.SendAsync(S1Commands.Backoffice(second, key: key)));

            Assert.Equal(20265, conflict.Code);
            Assert.Equal(409, conflict.HttpStatus);
            Assert.Equal(0, await CountAsync<OrderPreparation>(harness, preparation => preparation.SourceOfferId == second && preparation.ConsumedByOrderId != null));
            Assert.Equal(1, await CountAsync<Order>(harness, order => order.Id == created.OrderId));
        }

        [Fact]
        public async Task SC_S1_005_concurrent_requests_with_the_same_key_commit_exactly_one_order()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);
            var command = S1Commands.Backoffice(offerId);
            using var gate = new SemaphoreSlim(0);

            var attempts = Enumerable.Range(0, 6).Select(_ => Task.Run(async () =>
            {
                await gate.WaitAsync();
                return await harness.SendAsync(command);
            })).ToList();

            gate.Release(attempts.Count);
            var results = await Task.WhenAll(attempts);

            var orderId = results[0].OrderId;
            Assert.All(results, result => Assert.Equal(orderId, result.OrderId));
            Assert.Equal(1, await CountAsync<Order>(harness, order => order.Id == orderId));
            Assert.Equal(1, await CountAsync<CommandReceipt>(harness, receipt => receipt.OrderId == orderId));
            Assert.Equal(1, await harness.InScopeAsync(services => services.GetRequiredService<Query._Shared.DbContexts.OrderQueryDbContext>()
                .Set<OrderDetailsReadModel>().CountAsync(row => row.OrderId == orderId)));
        }

        [Fact]
        public async Task SC_S1_007_technical_stop_is_one_service_with_two_itinerary_legs()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await PublishAsync(harness, (now, scope) => new CandidateBuilder(now, scope)
                .Traveler("PAX-A")
                .Segment("SEG-A", "LEG-1", "LEG-2")
                .AirService("SERVICE-A", "PAX-A", "SEG-A")
                .Package("ITEM-A", "SERVICE-A")
                .Line("FARE", "ITEM-A", PricingComponentType.Fare, 100m, "SERVICE-A"));

            var created = await harness.SendAsync(S1Commands.Backoffice(offerId));
            var order = await harness.SendAsync(new BackofficeGetOrderByIdQuery(created.OrderId));

            Assert.Single(Assert.Single(order.Items).Services);
            Assert.Equal(2, Assert.Single(order.Itinerary).Legs.Count);
        }

        [Fact]
        public async Task SC_S1_008_source_priced_package_persists_one_item_with_four_services()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await PublishAsync(harness, (now, scope) => new CandidateBuilder(now, scope)
                .Traveler("PAX-A").Traveler("PAX-B")
                .Segment("SEG-1").Segment("SEG-2")
                .AirService("S-A1", "PAX-A", "SEG-1").AirService("S-A2", "PAX-A", "SEG-2")
                .AirService("S-B1", "PAX-B", "SEG-1").AirService("S-B2", "PAX-B", "SEG-2")
                .Package("PACKAGE", "S-A1", "S-A2", "S-B1", "S-B2")
                .Line("FARE-A", "PACKAGE", PricingComponentType.Fare, 150m, "S-A1")
                .Line("FARE-B", "PACKAGE", PricingComponentType.Fare, 150m, "S-B1"));

            var created = await harness.SendAsync(S1Commands.Backoffice(offerId, travellers: S1Commands.Travellers("PAX-A", "PAX-B")));
            var order = await LoadOrderAsync(harness, created.OrderId);

            Assert.Single(order.Items);
            Assert.Equal(4, order.Services.Count);
            Assert.Equal(4, order.ItemServiceLinks.Count);
            Assert.Equal(2, order.Travelers.Count);
            Assert.Equal(300m, order.CustomerTotal.Amount);
        }

        [Fact]
        public async Task SC_S1_009_round_trip_and_two_one_way_constructions_are_persisted_as_supplied()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);

            var roundTripOffer = await PublishAsync(harness, (now, scope) => RoundTrip(now, scope,
                Unit("PU-RT", FarePricingUnitType.RoundTrip, ["OUT", "IN"], Component("FC-RT", "YRT", ["S-OUT", "S-IN"]))));
            var oneWayOffer = await PublishAsync(harness, (now, scope) => RoundTrip(now, scope,
                Unit("PU-OUT", FarePricingUnitType.OneWay, ["OUT"], Component("FC-OUT", "YOW", ["S-OUT"])),
                Unit("PU-IN", FarePricingUnitType.OneWay, ["IN"], Component("FC-IN", "YOW", ["S-IN"]))));

            var roundTrip = await LoadOrderAsync(harness, (await harness.SendAsync(S1Commands.Backoffice(roundTripOffer))).OrderId);
            var oneWays = await LoadOrderAsync(harness, (await harness.SendAsync(S1Commands.Backoffice(oneWayOffer))).OrderId);

            var roundTripUnit = Assert.Single(roundTrip.FareConstructions.Single().PricingUnits);
            Assert.Equal(FarePricingUnitType.RoundTrip, roundTripUnit.Type);
            Assert.Equal(["IN", "OUT"], roundTripUnit.CoveredBounds.Select(bound => bound.SourceBoundRef).OrderBy(reference => reference));
            Assert.Equal(2, Assert.Single(roundTripUnit.Components).CoveredServices.Count);

            Assert.Equal(2, oneWays.FareConstructions.Single().PricingUnits.Count);
            Assert.All(oneWays.FareConstructions.Single().PricingUnits, unit => Assert.Equal(FarePricingUnitType.OneWay, unit.Type));
            Assert.All(oneWays.FareConstructions.Single().FareComponents, component => Assert.Single(component.CoveredServices));
            Assert.Equal(FareConstructionAssurance.SourceProvided, roundTrip.FareConstructions.Single().Assurance);
        }

        [Fact]
        public async Task SC_S1_011_group_extended_line_is_persisted_once()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await PublishAsync(harness, (now, scope) => new CandidateBuilder(now, scope)
                .Traveler("PAX-A").Traveler("PAX-B")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1").AirService("S-B", "PAX-B", "SEG-1")
                .Package("PACKAGE", "S-A", "S-B")
                .Line("GROUP-FARE", "PACKAGE", PricingComponentType.Fare, 200m, "PACKAGE", basisType: PricingBasisType.OrderItem)
                .SourceProvidedConstruction(Unit("PU-1", FarePricingUnitType.OneWay, ["OUT"],
                    new CandidatePricingGroup(["PAX-A", "PAX-B"], PassengerTypeCode.ADT, 2),
                    Component("FC-1", "YOW", ["S-A", "S-B"]))));

            var created = await harness.SendAsync(S1Commands.Backoffice(offerId, travellers: S1Commands.Travellers("PAX-A", "PAX-B")));
            var order = await LoadOrderAsync(harness, created.OrderId);
            var group = Assert.Single(order.FareConstructions.Single().PricingGroups);

            Assert.Equal(200m, order.CustomerTotal.Amount);
            Assert.Equal(200m, Assert.Single(order.PricingLines).SaleValue.Amount);
            Assert.Equal(2, group.Quantity);
            Assert.Equal(2, group.Travelers.Count);
            Assert.Equal(group.Id, Assert.Single(order.FareConstructions.Single().PricingUnits).PricingGroupId);
        }

        [Fact]
        public async Task SC_S1_012_original_and_sale_valuations_round_trip_exactly()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await PublishAsync(harness, (now, scope) => new CandidateBuilder(now, scope)
                .Traveler("PAX-A")
                .Segment("SEG-A")
                .AirService("SERVICE-A", "PAX-A", "SEG-A")
                .Package("ITEM-A", "SERVICE-A")
                .Line("FARE", "ITEM-A", PricingComponentType.Fare, 92m, "SERVICE-A", original: new Money(100m, "USD"))
                .Line("TAX", "ITEM-A", PricingComponentType.Tax, 1.23456789m, "SERVICE-A"));

            var order = await LoadOrderAsync(harness, (await harness.SendAsync(S1Commands.Backoffice(offerId))).OrderId);
            var fare = order.PricingLines.Single(line => line.Component == PricingComponentType.Fare);

            Assert.Equal(93.23456789m, order.CustomerTotal.Amount);
            Assert.Equal("EUR", order.CustomerTotal.CurrencyRef);
            Assert.Equal(100m, fare.OriginalValue.Amount);
            Assert.Equal("USD", fare.OriginalValue.CurrencyRef);
            Assert.Equal(92m, fare.SaleValue.Amount);
            Assert.Equal(1.23456789m, order.PricingLines.Single(line => line.Component == PricingComponentType.Tax).SaleValue.Amount);
        }

        [Fact]
        public async Task SC_S1_016_invalid_traveler_binding_is_rejected_before_any_order()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await PublishAsync(harness, (now, scope) => new CandidateBuilder(now, scope)
                .Traveler("PAX-A").Traveler("PAX-B")
                .Segment("SEG-1")
                .AirService("S-A", "PAX-A", "SEG-1").AirService("S-B", "PAX-B", "SEG-1")
                .Package("PACKAGE", "S-A", "S-B")
                .Line("FARE", "PACKAGE", PricingComponentType.Fare, 200m, "PACKAGE", basisType: PricingBasisType.OrderItem));

            var duplicated = S1Commands.Travellers("PAX-A", "PAX-A");
            var exception = await Assert.ThrowsAsync<BusinessException>(() => harness.SendAsync(S1Commands.Backoffice(offerId, travellers: duplicated)));

            Assert.Equal(20276, exception.Code);
            Assert.Equal(422, exception.HttpStatus);
            Assert.Equal(0, await CountAsync<Order>(harness, order => order.AcceptedSource.SourceOfferId == offerId));
        }

        [Fact]
        public async Task SC_S1_017_another_customer_can_neither_read_the_order_nor_see_the_offer()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);
            var created = await harness.SendAsync(S1Commands.Backoffice(offerId));

            harness.Caller.UseOta(S1Harness.OtherCustomerId, S1Harness.PartnerApiAccessProfileId);

            var foreignRead = await Assert.ThrowsAsync<BusinessException>(() =>
                harness.SendAsync(new Query.OrderAggregate.Queries.GetOrderById.Ota.OtaGetOrderByIdQuery(created.OrderId)));
            var foreignOffer = await Assert.ThrowsAsync<BusinessException>(() => harness.SendAsync(S1Commands.Ota(offerId)));

            Assert.Equal(20282, foreignRead.Code);
            Assert.Equal(404, foreignRead.HttpStatus);
            Assert.Equal(20274, foreignOffer.Code);

            harness.Caller.UseBackoffice(S1Harness.AirlineOfficeId, S1Harness.AirlineUserId);
            var own = await harness.SendAsync(new BackofficeGetOrderByIdQuery(created.OrderId));
            Assert.Equal(created.OrderId, own.OrderId);
        }

        [Fact]
        public async Task SC_S1_019_expired_owner_validity_blocks_the_sale_at_its_own_instant()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);

            harness.Clock.Advance(TimeSpan.FromMinutes(5));

            var expired = await Assert.ThrowsAsync<BusinessException>(() => harness.SendAsync(S1Commands.Backoffice(offerId)));

            Assert.Equal(20270, expired.Code);
            Assert.Contains("AirPrice", expired.Message);
            Assert.Equal(0, await CountAsync<Order>(harness, order => order.AcceptedSource.SourceOfferId == offerId));
        }

        [Fact]
        public async Task SC_S1_021_unregistered_product_schema_is_unsupported_before_any_order()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var candidate = CandidateBuilder.OneWayFare100Tax20(harness.Clock.GetDateTime(), S1Harness.Scope()).Offer(UniqueOffer()).Build();
            var unregistered = candidate with { Services = [candidate.Services[0] with { DetailSchemaVersion = 7 }] };
            await harness.Catalog.PublishAsync(unregistered);

            var exception = await Assert.ThrowsAsync<BusinessException>(() => harness.SendAsync(S1Commands.Backoffice(unregistered.Source.OfferId)));

            Assert.Equal(20273, exception.Code);
            Assert.Equal(0, await CountAsync<OrderPreparation>(harness, item => item.SourceOfferId == unregistered.Source.OfferId));
        }

        public static async Task<string> PublishAsync(S1Harness harness, Func<DateTimeOffset, AuthorizedSalesScope, CandidateBuilder> build)
        {
            var offerId = UniqueOffer();
            await harness.Catalog.PublishAsync(build(harness.Clock.GetDateTime(), S1Harness.Scope()).Offer(offerId).Build());
            return offerId;
        }

        public static async Task<CreateOrderFromOfferResult> CreateAsync(S1Harness harness, string offerId)
            => await harness.SendAsync(S1Commands.Backoffice(offerId));

        public static Task<Order> LoadOrderAsync(S1Harness harness, long orderId)
            => harness.InScopeAsync(async services =>
                await services.GetRequiredService<Domain.OrderAggregate.Contracts.IOrderRepository>().LoadSnapshotAsync(orderId, S1Harness.OwnerAirlineId)
                ?? throw new InvalidOperationException($"order {orderId} missing"));

        public static Task<int> CountAsync<TEntity>(S1Harness harness, System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate) where TEntity : class
            => harness.InScopeAsync(services => services.GetRequiredService<OrderingDbContext>().Set<TEntity>().AsNoTracking().CountAsync(predicate));

        public static string UniqueOffer() => $"REF-OFFER-{Guid.NewGuid():N}";

        private static CandidatePricingUnit Unit(
            string sourceUnitRef,
            FarePricingUnitType type,
            IReadOnlyList<string> coveredBounds,
            params CandidateFareComponent[] components)
            => new(sourceUnitRef, null, type, FareCombinationMethod.FiledFare, coveredBounds, null, components);

        private static CandidatePricingUnit Unit(
            string sourceUnitRef,
            FarePricingUnitType type,
            IReadOnlyList<string> coveredBounds,
            CandidatePricingGroup group,
            params CandidateFareComponent[] components)
            => new(sourceUnitRef, null, type, FareCombinationMethod.FiledFare, coveredBounds, group, components);

        private static CandidateFareComponent Component(string sourceFareRef, string fareBasis, IReadOnlyList<string> coveredServiceRefs)
            => new(sourceFareRef, fareBasis, null, "Published", null, null, "Y", null, null, null, null, null, coveredServiceRefs, []);

        private static CandidateBuilder RoundTrip(DateTimeOffset now, AuthorizedSalesScope scope, params CandidatePricingUnit[] units)
            => new CandidateBuilder(now, scope)
                .Traveler("PAX-A")
                .Journey("OUT").Segment("OUT-1").Journey("IN").Segment("IN-1")
                .AirService("S-OUT", "PAX-A", "OUT-1").AirService("S-IN", "PAX-A", "IN-1")
                .Package("PACKAGE", "S-OUT", "S-IN")
                .Line("FARE", "PACKAGE", PricingComponentType.Fare, 300m, "PACKAGE", basisType: PricingBasisType.OrderItem)
                .SourceProvidedConstruction(units);
    }
}
