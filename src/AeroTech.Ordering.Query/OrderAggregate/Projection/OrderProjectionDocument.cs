using AeroTech.Messages.Aegis.Enums;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Messages.Shared.Enums;

namespace AeroTech.Ordering.Query.OrderAggregate.Projection
{
    public sealed record OrderProjectionDocument(
        int SchemaVersion,
        long OrderId,
        string OrderReference,
        CommercialSummary Status,
        int CommercialVersion,
        string OfferId,
        SalesChannel Channel,
        long CustomerId,
        ProjectedSalesContext SalesContext,
        ProjectedParty? Buyer,
        ProjectedActor InitiatingActor,
        ProjectedMoney GrandTotal,
        string SaleCurrencyRef,
        string? SaleCurrencyCode,
        string? SourceJourneyTypeRaw,
        JourneyType? JourneyType,
        ProjectedObservedTime? ObservedTicketingDeadline,
        IReadOnlyList<ProjectedTraveller> Travellers,
        IReadOnlyList<ProjectedJourney> Journeys,
        IReadOnlyList<ProjectedSegment> Segments,
        IReadOnlyList<ProjectedItem> Items,
        IReadOnlyList<ProjectedPricingLine> Pricing,
        IReadOnlyList<ProjectedComponentTotal> ComponentTotals,
        IReadOnlyList<ProjectedFareConstruction> FareConstructions,
        IReadOnlyList<ProjectedItemServiceLink> ItemServiceLinks,
        DateTimeOffset CreationDate);

    public sealed record ProjectedSalesContext(
        ProjectedParty? Seller,
        SellingOfficeKind? SellingOfficeKind,
        long? SellingOfficeId);

    public sealed record ProjectedParty(BusinessContextType ContextType, long PartyId);

    public sealed record ProjectedActor(BusinessContextType ContextType, long? ActorId);

    public sealed record ProjectedComponentTotal(
        PricingComponentType Component,
        PricingEffect Effect,
        string DebitAmount,
        string CreditAmount,
        string CurrencyRef);

    public sealed record ProjectedMoney(string Amount, string CurrencyRef);

    public sealed record ProjectedObservedTime(DateTimeOffset Value, string SourceOwner, string SourceRef);

    public sealed record ProjectedTraveller(
        long TravellerId,
        string TravellerRef,
        string OfferTravellerRef,
        PassengerTypeCode PassengerType,
        long? GuardianTravellerId);

    public sealed record ProjectedJourney(
        long JourneyId,
        string SourceBoundRef,
        int Sequence,
        string? SourceDirectionRaw,
        BoundDirection? Direction,
        string OriginRef,
        string DestinationRef);

    public sealed record ProjectedSegment(
        long SegmentId,
        long? JourneyId,
        int Sequence,
        string SourceSegmentRef,
        SegmentKind Kind,
        string OriginRef,
        string? OriginTerminalRef,
        string DestinationRef,
        string? DestinationTerminalRef,
        DateTimeOffset? Departure,
        DateTimeOffset? Arrival,
        string? FlightRef,
        string? FlightNumber,
        string? FlightVersion,
        string? MarketingCarrierRef,
        string? OperatingCarrierRef,
        string? SourceCapacityRef,
        int? Duration,
        string? AircraftRef,
        IReadOnlyList<ProjectedLeg> Legs);

    public sealed record ProjectedLeg(
        long LegId,
        int Sequence,
        string SourceLegRef,
        string? OriginRef,
        string? OriginTerminalRef,
        string? DestinationRef,
        string? DestinationTerminalRef,
        DateTimeOffset? Departure,
        DateTimeOffset? Arrival);

    public sealed record ProjectedItem(
        long ItemId,
        string SourceItemRef,
        OrderItemKind Kind,
        OrderItemCommercialStatus Status,
        ProjectedMoney AcceptedTotal,
        ProjectedProduct Product,
        ProjectedCommercialTerms Terms,
        IReadOnlyList<ProjectedService> Services);

    public sealed record ProjectedProduct(
        string SourceSystem,
        string SourceOfferId,
        string? SourceOfferItemRef,
        string? ProductCode,
        string? ProductName,
        string? BrandCode,
        string? BrandName,
        string? ProductVersion);

    public sealed record ProjectedCommercialTerms(
        CommercialTermState Refundability,
        CommercialTermState Changeability,
        CommercialTermState UpgradeEligibility,
        string SourceSystem,
        string? SourcePolicyRef,
        string? SourcePolicyVersion,
        DateTimeOffset CapturedAt);

    public sealed record ProjectedService(
        long ServiceId,
        string SourceServiceRef,
        OrderServiceType ServiceType,
        OrderServiceCommercialStatus Status,
        string? ServiceCode,
        string? Name,
        ServicePriceTreatment PriceTreatment,
        string? SupplierPartyRef,
        string? DeliveryProviderRef,
        string Quantity,
        OrderItemUnitOfMeasure QuantityUnit,
        IReadOnlyList<long> TravellerIds,
        IReadOnlyList<long> SegmentIds,
        ProjectedSoldTerms SoldTerms,
        ProjectedFulfillmentProfile FulfillmentProfile,
        ProjectedAirTransport? AirTransport);

    public sealed record ProjectedSoldTerms(bool? Refundable, bool? Changeable, bool? Upgradable);

    public sealed record ProjectedFulfillmentProfile(
        string ProfileRef,
        string ProfileVersion,
        FulfillmentProfileAssurance Assurance,
        ReservationRequirement ReservationRequirement,
        FulfillmentDocumentKind DocumentKind,
        DocumentAuthority? DocumentAuthority,
        FundingRequirement FundingRequirement,
        int? CapacityUnits,
        string? ResourceUnitPolicyRef,
        string? DeliveryControlPolicyRef,
        string? DependencyTreatmentPolicyRef,
        bool? PartialFulfillmentSupported);

    public sealed record ProjectedAirTransport(
        string? CabinRef,
        string? RbdRef,
        string? BookingClass,
        ProjectedBaggage? CheckedBaggage,
        ProjectedBaggage? CabinBaggage);

    public sealed record ProjectedBaggage(int? Pieces, string? Weight, BaggageWeightUnit? WeightUnit);

    public sealed record ProjectedPricingLine(
        long LineId,
        long? ItemId,
        string SourceLineRef,
        PricingComponentType Component,
        PricingEffect Effect,
        OrderPricingLineDirection Direction,
        PricingLineRole Role,
        string? SourceCode,
        string? SourceName,
        string? SourceReference,
        PricingCalculationKind CalculationKind,
        ProjectedMoney SaleValue,
        ProjectedMoney OriginalValue,
        PricingBasisType BasisType,
        long? BasisId,
        string SourceBasisRef,
        string? SourceConversionRef,
        ProjectedConversion? AppliedConversion,
        ProjectedSettlementAttribution? SettlementAttribution);

    public sealed record ProjectedSettlementAttribution(string PartyRef, string CategoryCode);

    public sealed record ProjectedConversion(
        string SourceConversionRef,
        string FromCurrencyRef,
        string ToCurrencyRef,
        string Rate,
        int DecimalPlaces,
        string? RoundingToken);

    public sealed record ProjectedFareConstruction(
        long ConstructionId,
        FareConstructionAssurance Assurance,
        string SourceContextRef,
        IReadOnlyList<long> ItemIds,
        IReadOnlyList<ProjectedFareGroup> Groups,
        IReadOnlyList<ProjectedFareUnit> Units);

    public sealed record ProjectedFareGroup(
        long GroupId,
        PassengerTypeCode PassengerType,
        int Quantity,
        IReadOnlyList<long> TravellerIds);

    public sealed record ProjectedFareUnit(
        long UnitId,
        long? GroupId,
        int Sequence,
        string SourceUnitRef,
        string? SourceKindRaw,
        FarePricingUnitType Type,
        FareCombinationMethod CombinationMethod,
        IReadOnlyList<string> CoveredBoundRefs,
        IReadOnlyList<ProjectedFareComponent> Components);

    public sealed record ProjectedFareComponent(
        long ComponentId,
        int Sequence,
        string SourceFareRef,
        string? FareBasis,
        string? FareFamily,
        string? FareType,
        string? CabinRef,
        string? RbdRef,
        string? BookingClass,
        int? TicketingRestrictionMinutes,
        string? FareOwnerRef,
        string? TariffRef,
        string? RuleRef,
        string? RoutingRef,
        IReadOnlyList<long> CoveredServiceIds,
        IReadOnlyList<long> CoveredSegmentIds);

    public sealed record ProjectedItemServiceLink(
        long LinkId,
        long ItemId,
        long ServiceId,
        long LinkedByChangeId,
        IReadOnlyList<long> TravellerIdsAtAssociation,
        IReadOnlyList<long> SegmentIdsAtAssociation);
}
