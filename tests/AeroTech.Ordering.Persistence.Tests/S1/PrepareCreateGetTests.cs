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
using AeroTech.Ordering.Query.OperationAggregate.Queries.GetOperation;
using AeroTech.Ordering.Query.OrderAggregate.Models;
using AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.S1
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class PrepareCreateGetTests
    {
        private readonly OrderingDatabaseFixture _fixture;

        public PrepareCreateGetTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task SC_S1_002_reference_create_commits_one_item_one_service_total_120_and_no_fulfillment_effects()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);

            var preparation = await harness.SendAsync(S1Commands.Prepare(offerId));
            var created = await harness.SendAsync(S1Commands.Create(preparation, harness.Clock.GetDateTime()));

            var order = await LoadOrderAsync(harness, created.OrderId);
            Assert.Single(order.Items);
            Assert.Single(order.Services);
            Assert.Equal(2, order.PricingLines.Count);
            Assert.Equal(120.00m, order.CustomerTotal.Amount);
            Assert.Equal(1, order.CommercialVersion);
            Assert.Equal(1, order.FinancialSequence);
            Assert.Equal(1, order.OrderRevision);
            Assert.Equal(1, created.CommercialVersionAtCommit);
            Assert.False(created.ReplayedFromReceipt);
            Assert.Equal(preparation.AcceptedSnapshotDigest, created.AcceptedSourceDigest);
            Assert.Single(order.FundingObligations);

            await using var scope = harness.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();

            Assert.Equal(created.OrderId, (await db.Set<OrderPreparation>().AsNoTracking().SingleAsync(item => item.Id == preparation.PreparationId)).ConsumedByOrderId);
            Assert.Equal(CommandReceiptStatus.Completed, (await db.Set<CommandReceipt>().AsNoTracking().SingleAsync(receipt => receipt.OrderId == created.OrderId)).Status);

            var outbox = await db.OutboxMessages.AsNoTracking().SingleAsync(message => message.StreamId == created.OrderId);
            Assert.Equal("Order", outbox.StreamKind);
            Assert.Equal(1, outbox.EventOrdinal);
            Assert.Contains("OrderCreated", outbox.MessageType);
            Assert.Contains("\"CommercialVersion\":1", outbox.Payload);
            Assert.Contains("\"FinancialSequence\":1", outbox.Payload);

            var details = await harness.SendAsync(new GetOrderQuery(created.OrderId, OrderingApiSurface.Backoffice, 1));
            Assert.Equal(1, details.OrderRevision);
            Assert.Equal("120", details.Details.GetProperty("customerTotal").GetProperty("amount").GetString());
            Assert.Empty(details.Details.GetProperty("activeOperations").EnumerateArray());
            Assert.Empty(details.Details.GetProperty("facets").GetProperty("capacity").EnumerateArray());
            Assert.Empty(details.Details.GetProperty("facets").GetProperty("documents").EnumerateArray());
        }

        [Fact]
        public async Task SC_S1_001_owner_reprice_after_preparation_does_not_change_the_created_order_and_create_calls_no_owner()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);

            var preparation = await harness.SendAsync(S1Commands.Prepare(offerId));

            await harness.Catalog.PublishAsync(new CandidateBuilder(harness.Clock.GetDateTime(), S1Harness.Scope())
                .Offer(offerId)
                .Traveler("PAX-A")
                .Segment("SEG-A")
                .AirService("SERVICE-A", "PAX-A", "SEG-A")
                .Package("ITEM-A", "SERVICE-A")
                .Line("PRICE-FARE", "ITEM-A", PricingComponentType.Fare, 130.00m, "SERVICE-A")
                .Line("PRICE-TAX", "ITEM-A", PricingComponentType.Tax, 20.00m, "SERVICE-A")
                .Build());

            var readsBeforeCreate = await harness.Catalog.CountReadsAsync(offerId);
            var created = await harness.SendAsync(S1Commands.Create(preparation, harness.Clock.GetDateTime()));

            Assert.Equal(readsBeforeCreate, await harness.Catalog.CountReadsAsync(offerId));
            Assert.Equal(120.00m, (await LoadOrderAsync(harness, created.OrderId)).CustomerTotal.Amount);

            var repriced = await harness.SendAsync(S1Commands.Prepare(offerId));
            Assert.Equal(150.00m, repriced.Candidate.CustomerTotal.Amount);
            Assert.NotEqual(preparation.AcceptedSnapshotDigest, repriced.AcceptedSnapshotDigest);
        }

        [Fact]
        public async Task SC_S1_003_replay_after_owner_outage_resolves_from_the_durable_receipt_without_owner_access()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var offerId = await PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20);
            var prepare = S1Commands.Prepare(offerId);
            var preparation = await harness.SendAsync(prepare);
            var create = S1Commands.Create(preparation, harness.Clock.GetDateTime());
            var created = await harness.SendAsync(create);

            await harness.Catalog.SetAvailabilityAsync(false);
            var readsDuringOutage = await harness.Catalog.CountReadsAsync();

            try
            {
                var replayedCreate = await harness.SendAsync(create);
                var replayedPrepare = await harness.SendAsync(prepare);

                Assert.True(replayedCreate.ReplayedFromReceipt);
                Assert.Equal(created.OrderId, replayedCreate.OrderId);
                Assert.Equal(created.OrderReference, replayedCreate.OrderReference);
                Assert.Equal(created.OperationId, replayedCreate.OperationId);
                Assert.True(replayedPrepare.ReplayedFromReceipt);
                Assert.Equal(preparation.PreparationId, replayedPrepare.PreparationId);
                Assert.Equal(readsDuringOutage, await harness.Catalog.CountReadsAsync());
                Assert.Equal(1, await CountAsync<Order>(harness, order => order.SourcePreparationId == preparation.PreparationId));

                var unavailable = await Assert.ThrowsAsync<BusinessException>(() => harness.SendAsync(S1Commands.Prepare(offerId)));
                Assert.Equal(20275, unavailable.Code);
            }
            finally
            {
                await harness.Catalog.SetAvailabilityAsync(true);
            }
        }

        [Fact]
        public async Task SC_S1_004_same_key_for_another_preparation_is_a_conflict_and_changes_nothing()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var preparationA = await harness.SendAsync(S1Commands.Prepare(await PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20)));
            var preparationB = await harness.SendAsync(S1Commands.Prepare(await PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20)));
            var key = S1Commands.NewKey("shared");

            var created = await harness.SendAsync(S1Commands.Create(preparationA, harness.Clock.GetDateTime(), key: key));
            var conflict = await Assert.ThrowsAsync<BusinessException>(() => harness.SendAsync(S1Commands.Create(preparationB, harness.Clock.GetDateTime(), key: key)));

            Assert.Equal(20265, conflict.Code);
            Assert.Equal(409, conflict.HttpStatus);
            Assert.Equal(0, await CountAsync<Order>(harness, order => order.SourcePreparationId == preparationB.PreparationId));
            Assert.Null((await LoadPreparationAsync(harness, preparationB.PreparationId)).ConsumedByOrderId);
            Assert.Equal(created.OrderId, (await LoadPreparationAsync(harness, preparationA.PreparationId)).ConsumedByOrderId);
        }

        [Fact]
        public async Task SC_S1_005_concurrent_creates_with_different_keys_commit_exactly_one_order()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var preparation = await harness.SendAsync(S1Commands.Prepare(await PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20)));
            using var gate = new SemaphoreSlim(0);

            var attempts = Enumerable.Range(0, 6).Select(index => Task.Run(async () =>
            {
                await gate.WaitAsync();

                try
                {
                    return (Result: await harness.SendAsync(S1Commands.Create(preparation, harness.Clock.GetDateTime(), key: S1Commands.NewKey($"race-{index}"))), Error: (BusinessException?)null);
                }
                catch (BusinessException exception)
                {
                    return (Result: (CreatedOrderResult?)null, Error: exception);
                }
            })).ToList();

            gate.Release(attempts.Count);
            var outcomes = await Task.WhenAll(attempts);

            var winners = outcomes.Where(outcome => outcome.Result is not null).ToList();
            Assert.Single(winners);
            Assert.All(outcomes.Where(outcome => outcome.Error is not null), outcome => Assert.Equal(20267, outcome.Error!.Code));
            Assert.Equal(1, await CountAsync<Order>(harness, order => order.SourcePreparationId == preparation.PreparationId));
            Assert.Equal(1, await CountAsync<CommandReceipt>(harness, receipt => receipt.PreparationId == preparation.PreparationId && receipt.CommandKind == OrderingCommandKind.CreateOrderFromOffer));
            Assert.Equal(1, await harness.InScopeAsync(services => services.GetRequiredService<Query._Shared.DbContexts.OrderQueryDbContext>()
                .Set<OrderDetailsReadModel>().CountAsync(row => row.OrderId == winners[0].Result!.OrderId)));
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

            var created = await CreateAsync(harness, offerId);
            var details = await harness.SendAsync(new GetOrderQuery(created.OrderId, OrderingApiSurface.Backoffice, null));

            Assert.Single(details.Details.GetProperty("services").EnumerateArray());
            var segment = Assert.Single(details.Details.GetProperty("soldItinerary").EnumerateArray());
            Assert.Equal(2, segment.GetProperty("legs").GetArrayLength());
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

            var order = await LoadOrderAsync(harness, (await CreateAsync(harness, offerId)).OrderId);

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

            var roundTrip = await LoadOrderAsync(harness, (await CreateAsync(harness, await PublishAsync(harness, (now, scope) => RoundTrip(now, scope,
                new CandidatePricingUnit("PU-RT", FarePricingUnitType.RoundTrip, FareCombinationMethod.FiledFare, ["OUT", "IN"], null,
                    [new CandidateFareComponent("FC-RT", "YRT", null, "Published", null, null, "Y", ["S-OUT", "S-IN"])]))))).OrderId);

            var oneWays = await LoadOrderAsync(harness, (await CreateAsync(harness, await PublishAsync(harness, (now, scope) => RoundTrip(now, scope,
                new CandidatePricingUnit("PU-OUT", FarePricingUnitType.OneWay, FareCombinationMethod.FiledFare, ["OUT"], null,
                    [new CandidateFareComponent("FC-OUT", "YOW", null, "Published", null, null, "Y", ["S-OUT"])]),
                new CandidatePricingUnit("PU-IN", FarePricingUnitType.OneWay, FareCombinationMethod.FiledFare, ["IN"], null,
                    [new CandidateFareComponent("FC-IN", "YOW", null, "Published", null, null, "Y", ["S-IN"])]))))).OrderId);

            Assert.Contains("\"type\":\"RoundTrip\"", roundTrip.FareConstructions.Single().PricingUnitsJson);
            Assert.Contains("\"type\":\"OneWay\"", oneWays.FareConstructions.Single().PricingUnitsJson);
            Assert.NotEqual(roundTrip.FareConstructions.Single().PricingUnitsJson, oneWays.FareConstructions.Single().PricingUnitsJson);
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
                .SourceProvidedConstruction(new CandidatePricingUnit("PU-1", FarePricingUnitType.OneWay, FareCombinationMethod.FiledFare, ["OUT"],
                    new CandidatePricingGroup(["PAX-A", "PAX-B"], "ADT", 2),
                    [new CandidateFareComponent("FC-1", "YOW", null, "Published", null, null, "Y", ["S-A", "S-B"])])));

            var order = await LoadOrderAsync(harness, (await CreateAsync(harness, offerId)).OrderId);

            Assert.Equal(200m, order.CustomerTotal.Amount);
            Assert.Equal(200m, Assert.Single(order.PricingLines).SaleValue.Amount);
            Assert.Contains("\"quantity\":2", order.FareConstructions.Single().PricingUnitsJson);
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

            var order = await LoadOrderAsync(harness, (await CreateAsync(harness, offerId)).OrderId);
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
            var preparation = await harness.SendAsync(S1Commands.Prepare(offerId));
            var bindings = S1Commands.BindAll(preparation.Candidate);
            var duplicated = new[] { bindings[0], bindings[0] with { ClientTravelerRef = "CLIENT-OTHER" } };

            var exception = await Assert.ThrowsAsync<BusinessException>(() => harness.SendAsync(S1Commands.Create(preparation, harness.Clock.GetDateTime(), bindings: duplicated)));

            Assert.Equal(20276, exception.Code);
            Assert.Equal(422, exception.HttpStatus);
            Assert.Null((await LoadPreparationAsync(harness, preparation.PreparationId)).ConsumedByOrderId);
            Assert.Equal(0, await CountAsync<Order>(harness, order => order.SourcePreparationId == preparation.PreparationId));
        }

        [Fact]
        public async Task SC_S1_017_other_customer_cannot_reach_preparation_order_operation_or_receipt()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var preparation = await harness.SendAsync(S1Commands.Prepare(await PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20)));
            var key = S1Commands.NewKey("customer-a");
            var created = await harness.SendAsync(S1Commands.Create(preparation, harness.Clock.GetDateTime(), key: key));
            var customerB = S1Harness.Sale(S1Harness.OtherCustomerId, null, OrderingApiSurface.Ota);

            harness.Caller.UseOta(S1Harness.OtherCustomerId, S1Harness.PartnerApiAccessProfileId);

            var replay = await Assert.ThrowsAsync<BusinessException>(() => harness.SendAsync(S1Commands.Create(preparation, harness.Clock.GetDateTime(), customerB, key)));
            var order = await Assert.ThrowsAsync<BusinessException>(() => harness.SendAsync(new GetOrderQuery(created.OrderId, OrderingApiSurface.Ota, null)));
            var operation = await Assert.ThrowsAsync<BusinessException>(() => harness.SendAsync(new GetOperationQuery(created.OperationId, OrderingApiSurface.Ota)));
            var foreignOffer = await Assert.ThrowsAsync<BusinessException>(() => harness.SendAsync(S1Commands.Prepare(preparation.SourceOfferId, customerB)));

            Assert.Equal(20266, replay.Code);
            Assert.Equal(404, replay.HttpStatus);
            Assert.Equal(20282, order.Code);
            Assert.Equal(20283, operation.Code);
            Assert.Equal(20274, foreignOffer.Code);
            Assert.DoesNotContain(created.OrderId.ToString(), replay.Message);

            var missingOrder = await Assert.ThrowsAsync<BusinessException>(() => harness.SendAsync(new GetOrderQuery(created.OrderId + 1_000_003, OrderingApiSurface.Ota, null)));
            var missingOperation = await Assert.ThrowsAsync<BusinessException>(() => harness.SendAsync(new GetOperationQuery(created.OperationId + 1_000_003, OrderingApiSurface.Ota)));
            Assert.Equal((missingOrder.Code, missingOrder.HttpStatus), (order.Code, order.HttpStatus));
            Assert.Equal((missingOperation.Code, missingOperation.HttpStatus), (operation.Code, operation.HttpStatus));
            Assert.Equal(missingOrder.Message.Replace((created.OrderId + 1_000_003).ToString(), "{id}"), order.Message.Replace(created.OrderId.ToString(), "{id}"));

            harness.Caller.UseBackoffice(S1Harness.AirlineOfficeId, S1Harness.AirlineUserId);
            var own = await harness.SendAsync(new GetOperationQuery(created.OperationId, OrderingApiSurface.Backoffice));
            Assert.Equal(created.OrderId, own.OrderId);
            Assert.Equal("Completed", own.Phase);
        }

        [Fact]
        public async Task SC_S1_019_expired_owner_validity_blocks_acceptance_at_its_own_instant()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var preparation = await harness.SendAsync(S1Commands.Prepare(await PublishAsync(harness, CandidateBuilder.OneWayFare100Tax20)));

            harness.Clock.Advance(TimeSpan.FromMinutes(5));

            var expired = await Assert.ThrowsAsync<BusinessException>(() => harness.SendAsync(S1Commands.Create(preparation, harness.Clock.GetDateTime())));

            Assert.Equal(20270, expired.Code);
            Assert.Contains("AirPrice", expired.Message);
            Assert.Equal(ValidityState.Known, preparation.OfferValidity.State);
            Assert.Equal(ValidityState.NotSupplied, preparation.TicketingValidity.State);
            Assert.NotEqual(preparation.OfferValidity.Value, preparation.PriceValidity.Value);
        }

        [Fact]
        public async Task SC_S1_021_unregistered_product_schema_is_unsupported_before_any_preparation()
        {
            await using var harness = await S1Harness.StartAsync(_fixture);
            var candidate = CandidateBuilder.OneWayFare100Tax20(harness.Clock.GetDateTime(), S1Harness.Scope()).Offer(UniqueOffer()).Build();
            var unregistered = candidate with { Services = [candidate.Services[0] with { DetailSchemaVersion = 7 }] };
            await harness.Catalog.PublishAsync(unregistered);

            var exception = await Assert.ThrowsAsync<BusinessException>(() => harness.SendAsync(S1Commands.Prepare(unregistered.Source.OfferId)));

            Assert.Equal(20273, exception.Code);
            Assert.Equal(0, await CountAsync<OrderPreparation>(harness, item => item.SourceOfferId == unregistered.Source.OfferId));
        }

        public static async Task<string> PublishAsync(S1Harness harness, Func<DateTimeOffset, AuthorizedSalesScope, CandidateBuilder> build)
        {
            var offerId = UniqueOffer();
            await harness.Catalog.PublishAsync(build(harness.Clock.GetDateTime(), S1Harness.Scope()).Offer(offerId).Build());
            return offerId;
        }

        public static async Task<CreatedOrderResult> CreateAsync(S1Harness harness, string offerId)
        {
            var preparation = await harness.SendAsync(S1Commands.Prepare(offerId));
            return await harness.SendAsync(S1Commands.Create(preparation, harness.Clock.GetDateTime()));
        }

        public static Task<Order> LoadOrderAsync(S1Harness harness, long orderId)
            => harness.InScopeAsync(async services =>
                await services.GetRequiredService<Domain.OrderAggregate.Contracts.IOrderRepository>().LoadSnapshotAsync(orderId, S1Harness.OwnerAirlineId)
                ?? throw new InvalidOperationException($"order {orderId} missing"));

        public static Task<OrderPreparation> LoadPreparationAsync(S1Harness harness, long preparationId)
            => harness.InScopeAsync(services => services.GetRequiredService<OrderingDbContext>().Set<OrderPreparation>().AsNoTracking().SingleAsync(item => item.Id == preparationId));

        public static Task<int> CountAsync<TEntity>(S1Harness harness, System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate) where TEntity : class
            => harness.InScopeAsync(services => services.GetRequiredService<OrderingDbContext>().Set<TEntity>().AsNoTracking().CountAsync(predicate));

        public static string UniqueOffer() => $"REF-OFFER-{Guid.NewGuid():N}";

        private static CandidateBuilder RoundTrip(DateTimeOffset now, AuthorizedSalesScope scope, params CandidatePricingUnit[] units)
            => new CandidateBuilder(now, scope)
                .Traveler("PAX-A")
                .Segment("OUT-1").Segment("IN-1")
                .AirService("S-OUT", "PAX-A", "OUT-1").AirService("S-IN", "PAX-A", "IN-1")
                .Package("PACKAGE", "S-OUT", "S-IN")
                .Line("FARE", "PACKAGE", PricingComponentType.Fare, 300m, "PACKAGE", basisType: PricingBasisType.OrderItem)
                .SourceProvidedConstruction(units);
    }
}
