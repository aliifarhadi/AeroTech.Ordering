using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects
{
    public sealed record NormalizedCandidate(
        string SchemaVersion,
        CandidateSource Source,
        AcceptanceAssurance AcceptanceAssurance,
        DateTimeOffset PricedAt,
        DateTimeOffset CapturedAt,
        CandidateValidity Validity,
        CandidateSalesContext SalesContext,
        IReadOnlyList<CandidateTraveler> Travelers,
        IReadOnlyList<CandidateJourney> Journeys,
        IReadOnlyList<CandidateSegment> Segments,
        IReadOnlyList<CandidateItem> Items,
        IReadOnlyList<CandidateService> Services,
        IReadOnlyList<CandidatePricingLine> PricingLines,
        Money CustomerTotal,
        string? SaleCurrencyCode,
        string? SourceJourneyTypeRaw,
        JourneyType? JourneyType,
        CandidateFareConstruction FareConstruction)
    {
        public const string CurrentSchemaVersion = "3.0";

        public CurrencySnapshot SaleCurrency => new(CustomerTotal.CurrencyRef, SaleCurrencyCode);
    }
}
