using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Domain._Shared.ValueObjects;
using AeroTech.Ordering.Domain.OrderAggregate.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.Policies;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class OrderService : Entity<long>
    {
        private readonly List<OrderServiceBeneficiary> _beneficiaries = new();
        private readonly List<OrderServiceCoverage> _coverage = new();

        private OrderService()
        {
        }

        internal OrderService(
            long id,
            long orderId,
            long orderItemId,
            CandidateService source,
            IReadOnlyDictionary<string, long> travelerIds,
            IReadOnlyDictionary<string, long> segmentIds,
            long createdByChangeId,
            Func<long> newId)
        {
            Id = id;
            OrderId = orderId;
            OrderItemId = orderItemId;
            SourceServiceRef = source.ServiceRef;
            Type = source.Type;
            ServiceCode = source.ServiceCode;
            Name = source.Name;
            PriceTreatment = source.PriceTreatment;
            SupplierPartyRef = source.SupplierPartyRef;
            DeliveryProviderRef = source.DeliveryProviderRef;
            CommercialStatus = OrderServiceCommercialStatus.Active;
            ServiceVersion = 1;
            Quantity = source.Quantity;
            QuantityUnit = source.QuantityUnit;
            DetailSchema = source.DetailSchema;
            DetailSchemaVersion = source.DetailSchemaVersion;
            SoldTerms = source.SoldTerms;
            FulfillmentProfile = new FulfillmentProfileSnapshot(
                source.FulfillmentProfile.ProfileRef,
                source.FulfillmentProfile.ProfileVersion,
                source.FulfillmentProfile.Assurance,
                source.FulfillmentProfile.ReservationRequirement,
                source.FulfillmentProfile.DocumentKind,
                source.FulfillmentProfile.DocumentAuthority,
                source.FulfillmentProfile.FundingRequirement,
                source.FulfillmentProfile.CapacityUnits,
                source.FulfillmentProfile.ResourceUnitPolicyRef,
                source.FulfillmentProfile.DeliveryControlPolicyRef,
                source.FulfillmentProfile.DependencyTreatmentPolicyRef,
                source.FulfillmentProfile.PartialFulfillmentSupported);
            CreatedByChangeId = createdByChangeId;

            foreach (var beneficiary in source.BeneficiaryRefs)
                _beneficiaries.Add(new OrderServiceBeneficiary(newId(), id, travelerIds[beneficiary]));

            foreach (var segment in source.SegmentRefs)
                _coverage.Add(new OrderServiceCoverage(newId(), id, segmentIds[segment]));

            if (Type == OrderServiceType.AirTransportation)
                AirTransport = new AirTransportDetail(
                    Detail(source, ServiceDetailSchemaRegistry.CabinRef),
                    Detail(source, ServiceDetailSchemaRegistry.RbdRef),
                    Detail(source, ServiceDetailSchemaRegistry.BookingClass),
                    source.CheckedBaggage,
                    source.CabinBaggage);
        }

        public long OrderId { get; private set; }

        public long OrderItemId { get; private set; }

        public string SourceServiceRef { get; private set; } = null!;

        public OrderServiceType Type { get; private set; }

        public string? ServiceCode { get; private set; }

        public string? Name { get; private set; }

        public ServicePriceTreatment PriceTreatment { get; private set; }

        public string? SupplierPartyRef { get; private set; }

        public string? DeliveryProviderRef { get; private set; }

        public OrderServiceCommercialStatus CommercialStatus { get; private set; }

        public int ServiceVersion { get; private set; }

        public decimal Quantity { get; private set; }

        public OrderItemUnitOfMeasure QuantityUnit { get; private set; }

        public string DetailSchema { get; private set; } = null!;

        public int DetailSchemaVersion { get; private set; }

        public SoldTermFlags SoldTerms { get; private set; } = null!;

        public FulfillmentProfileSnapshot FulfillmentProfile { get; private set; } = null!;

        public long CreatedByChangeId { get; private set; }

        public AirTransportDetail? AirTransport { get; private set; }

        public IReadOnlyCollection<OrderServiceBeneficiary> Beneficiaries => _beneficiaries.AsReadOnly();

        public IReadOnlyCollection<OrderServiceCoverage> Coverage => _coverage.AsReadOnly();

        public long SoleCoveredSegmentId => _coverage.Count == 1
            ? _coverage[0].SegmentId
            : throw ExceptionFactory.ServiceCoverageIsNotASingleSegment(Id, _coverage.Count);

        private static string? Detail(CandidateService source, string key)
            => source.Details.TryGetValue(key, out var value) ? value : null;
    }
}
