using System.Net;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderPreparationAggregate;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Contracts;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;
using AeroTech.Ordering.Domain.Ports.Offers;
using AeroTech.Ordering.Persistence.Tests._Shared;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Providers.AirOffer;
using AeroTech.Ordering.Providers.AirOffer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.S1
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class AirOfferLiveCandidateBridgeTests
    {
        private const string OfferId = "SYNTHETIC-PRICED-OFFER";

        private readonly OrderingDatabaseFixture _fixture;

        public AirOfferLiveCandidateBridgeTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task SC_S1_010_observed_details_become_one_conservative_package_with_opaque_construction()
        {
            var handler = new AirOfferWireFixtures.StubHandler(() => AirOfferWireFixtures.Details());
            await using var harness = await StartAsync(handler);

            var created = await harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1")));
            var candidate = await AcceptedCandidateAsync(harness, created.OrderId);

            Assert.Equal("/service/v1/FlightOffers/Details", handler.LastRequestPath);
            Assert.Contains("\"offerId\":\"SYNTHETIC-PRICED-OFFER\"", handler.LastRequestBody);

            var item = Assert.Single(candidate.Items);
            Assert.Equal(OrderItemKind.OfferPackage, item.ItemKind);
            Assert.Null(item.SourceOfferItemRef);
            Assert.Single(candidate.Services);
            Assert.Equal(FareConstructionAssurance.Opaque, candidate.FareConstruction.Assurance);
            Assert.All(candidate.FareConstruction.PricingUnits.SelectMany(unit => unit.Components), component => Assert.Empty(component.CoveredServiceRefs));
            Assert.Equal(["tickets/0/coupons/0/pricings/0", "tickets/0/coupons/0/pricings/1"], candidate.PricingLines.Select(line => line.SourceLineRef));
            Assert.Equal([PricingComponentType.Fare, PricingComponentType.Tax], candidate.PricingLines.Select(line => line.Component));
            Assert.Equal(120m, candidate.CustomerTotal.Amount);
            Assert.Equal(2, Assert.Single(candidate.Segments).Legs.Count);
        }

        [Fact]
        public async Task SC_S1_015_hierarchy_total_mismatch_is_contract_mismatch_and_persists_nothing()
        {
            var handler = new AirOfferWireFixtures.StubHandler(() => AirOfferWireFixtures.Details(rootTotal: 119m));
            await using var harness = await StartAsync(handler);
            var key = S1Commands.NewKey("mismatch");

            var exception = await Assert.ThrowsAsync<BusinessException>(() =>
                harness.SendAsync(S1Commands.Backoffice(OfferId, key: key, travellers: S1Commands.Travellers("T1"))));

            Assert.Equal(20272, exception.Code);
            Assert.Contains("offer total 119", exception.Message);
            Assert.Equal(0, await CreateOrderFromOfferTests.CountAsync<Domain.CommandReceiptAggregate.CommandReceipt>(harness, receipt => receipt.IdempotencyKey == key));
        }

        [Fact]
        public async Task SC_S1_018_live_candidate_keeps_not_supplied_validity_and_is_refused_in_production()
        {
            var handler = new AirOfferWireFixtures.StubHandler(() => AirOfferWireFixtures.Details());
            await using var harness = await StartAsync(handler);

            var callsBeforeCreate = handler.Calls;
            var created = await harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1")));
            var order = await CreateOrderFromOfferTests.LoadOrderAsync(harness, created.OrderId);
            var preparation = await AcceptedPreparationAsync(harness, created.OrderId);

            Assert.Equal(callsBeforeCreate + 1, handler.Calls);
            Assert.Equal(AcceptanceAssurance.LocalCandidateOnly, preparation.AcceptanceAssurance);
            Assert.Equal(ValidityState.NotSupplied, preparation.OfferValidity.State);
            Assert.Equal(ValidityState.NotSupplied, preparation.PriceValidity.State);
            Assert.Equal(ValidityState.NotSupplied, preparation.TicketingValidity.State);
            Assert.Contains("BD-004", preparation.TicketingValidity.Reason);
            Assert.DoesNotContain("LastTicketingDate", preparation.TicketingValidity.Reason);
            var observed = Assert.IsType<ObservedTimeFact>(order.ObservedTicketingDeadline);
            Assert.Equal(AirOfferProfile.Owner, observed.SourceOwner);
            Assert.Equal(AirOfferCandidateMapper.TicketingDeadlineSourceRef, observed.SourceRef);
            Assert.True(order.IsSandboxScoped);
            Assert.Equal(AirOfferProfile.LiveCandidateSandbox, order.AcceptedSource.AcceptanceProfile);

            await using var production = await StartAsync(handler, productionPolicy: true);
            var refused = await Assert.ThrowsAsync<BusinessException>(() =>
                production.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1"))));

            Assert.Equal(20269, refused.Code);
        }

        [Theory]
        [InlineData("unknown-category")]
        [InlineData("percentage-without-sale-valuation")]
        [InlineData("no-sale-valuation")]
        [InlineData("infant")]
        public async Task Observed_wire_violations_fail_mapping_instead_of_guessing(string defect)
        {
            var body = defect switch
            {
                "unknown-category" => AirOfferWireFixtures.Details(category: 9),
                "percentage-without-sale-valuation" => AirOfferWireFixtures.Details(percentage: true)
                    .Replace("\"isPercentage\":true,\"equivalentAmount\":100,\"equivalentCurrencyId\":978", "\"isPercentage\":true,\"equivalentAmount\":100,\"equivalentCurrencyId\":826"),
                "no-sale-valuation" => AirOfferWireFixtures.Details(lineCurrencyId: 840, equivalent: 100m).Replace("\"equivalentCurrencyId\":978,\"rateOfExchangePeriodId\":\"ROE-1\"", "\"equivalentCurrencyId\":826,\"rateOfExchangePeriodId\":\"ROE-1\""),
                _ => AirOfferWireFixtures.Details(passengerType: "INF")
            };

            var handler = new AirOfferWireFixtures.StubHandler(() => body);
            await using var harness = await StartAsync(handler);

            var exception = await Assert.ThrowsAsync<BusinessException>(() =>
                harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1"))));

            Assert.Equal(defect == "infant" ? 20273 : 20272, exception.Code);
        }

        [Fact]
        public async Task Equivalent_amount_is_used_as_sale_value_and_the_original_is_retained()
        {
            var handler = new AirOfferWireFixtures.StubHandler(() => AirOfferWireFixtures.Details(couponFare: 100m, lineCurrencyId: 840, equivalent: 92m)
                .Replace("\"baseAmount\":100,\"chargeAmount\":20,\"totalAmount\":120", "\"baseAmount\":92,\"chargeAmount\":20,\"totalAmount\":112"));
            await using var harness = await StartAsync(handler);

            var created = await harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1")));
            var candidate = await AcceptedCandidateAsync(harness, created.OrderId);
            var fare = candidate.PricingLines.Single(line => line.Component == PricingComponentType.Fare);

            Assert.Equal(92m, fare.SaleValue.Amount);
            Assert.Equal("978", fare.SaleValue.CurrencyRef);
            Assert.Equal(100m, fare.OriginalValue.Amount);
            Assert.Equal("840", fare.OriginalValue.CurrencyRef);
            Assert.Equal("ROE-1", fare.SourceConversionRef);
            Assert.Equal(112m, candidate.CustomerTotal.Amount);
        }

        [Fact]
        public async Task Percentage_order_charge_is_valued_from_its_sale_currency_equivalent()
        {
            var handler = new AirOfferWireFixtures.StubHandler(() => AirOfferWireFixtures.Details(percentageOrderCharge: 24m));
            await using var harness = await StartAsync(handler);

            var created = await harness.SendAsync(S1Commands.Backoffice(OfferId, travellers: S1Commands.Travellers("T1")));
            var candidate = await AcceptedCandidateAsync(harness, created.OrderId);
            var charge = candidate.PricingLines.Single(line => line.SourceLineRef == "orderCharges/0");

            Assert.Equal(PricingComponentType.Tax, charge.Component);
            Assert.Equal(24m, charge.SaleValue.Amount);
            Assert.Equal("978", charge.SaleValue.CurrencyRef);
            Assert.Equal(24m, charge.OriginalValue.Amount);
            Assert.Equal("978", charge.OriginalValue.CurrencyRef);
            Assert.Equal("70", charge.SourceConversionRef);
            Assert.Equal(PricingBasisType.OrderItem, charge.BasisType);
            Assert.Equal(144m, candidate.CustomerTotal.Amount);
        }

        [Fact]
        public async Task Owner_server_error_is_unavailable_and_client_error_is_not_found()
        {
            var status = HttpStatusCode.ServiceUnavailable;
            var handler = new AirOfferWireFixtures.StubHandler(() => "{\"data\":null,\"errors\":[{\"code\":1,\"title\":\"x\"}]}", () => status);
            await using var harness = await StartAsync(handler);

            Assert.Equal(20275, (await Assert.ThrowsAsync<BusinessException>(() => harness.SendAsync(S1Commands.Backoffice(OfferId)))).Code);

            status = HttpStatusCode.BadRequest;
            Assert.Equal(20274, (await Assert.ThrowsAsync<BusinessException>(() => harness.SendAsync(S1Commands.Backoffice(OfferId)))).Code);

            var bound = await harness.InScopeAsync(services => services.GetRequiredService<IOfferSourcePort>()
                .ReadBoundCandidateAsync(new ReadBoundCandidateRequest("binding", new string('0', 64), S1Harness.Scope())));
            Assert.Equal(OfferResolutionOutcome.UnsupportedCapability, bound.Outcome);
        }

        public static Task<OrderPreparation> AcceptedPreparationAsync(S1Harness harness, long orderId)
            => harness.InScopeAsync(services => services.GetRequiredService<OrderingDbContext>()
                .Set<OrderPreparation>().AsNoTracking().SingleAsync(preparation => preparation.ConsumedByOrderId == orderId));

        public static async Task<NormalizedCandidate> AcceptedCandidateAsync(S1Harness harness, long orderId)
            => (await AcceptedPreparationAsync(harness, orderId)).Candidate;

        private Task<S1Harness> StartAsync(HttpMessageHandler handler, bool productionPolicy = false)
            => S1Harness.StartAsync(_fixture, services =>
            {
                services.ConfigureHttpClientDefaults(client => client.ConfigurePrimaryHttpMessageHandler(() => handler));

                if (productionPolicy)
                    services.Replace(ServiceDescriptor.Singleton<IAcceptanceProfilePolicy>(new ProductionPolicy()));
            }, useReferenceOffers: false);

        private sealed class ProductionPolicy : IAcceptanceProfilePolicy
        {
            public string EnvironmentClass => "Production";

            public bool Permits(string acceptanceProfile) => false;
        }
    }
}
