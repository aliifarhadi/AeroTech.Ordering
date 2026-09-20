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
        string SourceOfferId,
        SalesChannel Channel,
        long FinancialCustomerId,
        ProjectedSalesContext SalesContext,
        ProjectedActor InitiatingActor,
        int CurrencyId,
        string? SaleCurrencyCode,
        ProjectedMoney CustomerTotal,
        JourneyType JourneyType,
        DateTimeOffset? LastTicketingDate,
        IReadOnlyList<ProjectedTraveller> Travellers,
        IReadOnlyList<ProjectedJourney> Journeys,
        IReadOnlyList<ProjectedSegment> Segments,
        IReadOnlyList<ProjectedItem> Items,
        IReadOnlyList<ProjectedPricingLine> Pricing,
        IReadOnlyList<ProjectedComponentTotal> ComponentTotals,
        IReadOnlyList<ProjectedFareConstruction> FareConstructions,
        DateTimeOffset CreationDate);

    public sealed record ProjectedSalesContext(
        BusinessContextType? SellerContextType,
        long? SellerId,
        SellingOfficeKind? SellingOfficeKind,
        long? SellingOfficeId);

    public sealed record ProjectedActor(BusinessContextType ContextType, long? ActorId);

    public sealed record ProjectedComponentTotal(
        PricingComponentType Component,
        PricingEffect Effect,
        string DebitAmount,
        string CreditAmount,
        int CurrencyId);

    public sealed record ProjectedMoney(string Amount, int CurrencyId);

    public sealed record ProjectedTraveller(
        long TravellerId,
        string TravellerRef,
        string SourceTravellerRef,
        PassengerTypeCode PassengerType,
        long? InfantParentTravellerId);

    public sealed record ProjectedJourney(
        long JourneyId,
        string BoundId,
        int Sequence,
        BoundDirection Direction,
        int OriginAirportId,
        int DestinationAirportId);

    public sealed record ProjectedSegment(
        long SegmentId,
        long JourneyId,
        int Sequence,
        SegmentKind Kind,
        int OriginAirportId,
        int? OriginAirportTerminalId,
        int DestinationAirportId,
        int? DestinationAirportTerminalId,
        DateTimeOffset? SoldDeparture,
        DateTimeOffset? SoldArrival,
        long? FlightId,
        string? FlightNumber,
        int? FlightVersion,
        int? MarketingAirlineId,
        int? OperatingAirlineId,
        long? FlightCapacityId,
        int? Duration,
        int? AircraftId,
        IReadOnlyList<ProjectedLeg> Legs);

    public sealed record ProjectedLeg(
        long OrderSegmentLegId,
        long LegId,
        int Sequence,
        int? OriginAirportId,
        int? OriginAirportTerminalId,
        int? DestinationAirportId,
        int? DestinationAirportTerminalId,
        DateTimeOffset? DepartureDateTime,
        DateTimeOffset? ArrivalDateTime);

    public sealed record ProjectedItem(
        long ItemId,
        OrderItemKind Kind,
        OrderItemCommercialStatus Status,
        ProjectedMoney AcceptedTotal,
        IReadOnlyList<ProjectedService> Services);

    public sealed record ProjectedService(
        long ServiceId,
        OrderServiceType ServiceType,
        OrderServiceCommercialStatus Status,
        long TravellerId,
        long SegmentId,
        int? CabinClassId,
        long? RbdId,
        string? BookingClass,
        ProjectedBaggage? CheckedBaggage,
        ProjectedBaggage? CabinBaggage,
        bool? Refundable,
        bool? Changeable,
        bool? Upgradable);

    public sealed record ProjectedBaggage(int? Pieces, string? Weight, BaggageWeightUnit? WeightUnit);

    public sealed record ProjectedPricingLine(
        long LineId,
        long? ItemId,
        string SourceOccurrencePath,
        PricingComponentType Component,
        PricingEffect Effect,
        OrderPricingLineDirection Direction,
        PricingLineRole Role,
        string? Code,
        string? Name,
        string? Reference,
        PricingCalculationKind CalculationKind,
        ProjectedMoney SaleValue,
        ProjectedMoney OriginalValue,
        PricingBasisType BasisType,
        long? BasisId,
        string? SourceConversionRef,
        ProjectedConversion? AppliedConversion,
        ProjectedSettlementAttribution? SettlementAttribution);

    public sealed record ProjectedSettlementAttribution(string PartyRef, string CategoryCode);

    public sealed record ProjectedConversion(
        string SourceConversionRef,
        int FromCurrencyId,
        int ToCurrencyId,
        string Rate,
        int DecimalPlaces,
        string? RoundingToken);

    public sealed record ProjectedFareConstruction(
        long ConstructionId,
        FareConstructionAssurance Assurance,
        IReadOnlyList<long> ItemIds,
        IReadOnlyList<ProjectedFareUnit> Units);

    public sealed record ProjectedFareUnit(
        long UnitId,
        int Sequence,
        FarePricingUnitType Type,
        AirFareConstructionType SourceConstructionType,
        IReadOnlyList<string> CoveredBoundOfferIds,
        IReadOnlyList<ProjectedFareComponent> Components);

    public sealed record ProjectedFareComponent(
        long ComponentId,
        int Sequence,
        long AirFareId,
        string? FareBasis,
        string? FareFamily,
        string? FareType,
        int? CabinClassId,
        long? RbdId,
        string? BookingClass,
        int? TicketingRestrictionMinutes);
}
