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
        IReadOnlyList<CandidateSegment> Segments,
        IReadOnlyList<CandidateItem> Items,
        IReadOnlyList<CandidateService> Services,
        IReadOnlyList<CandidatePricingLine> PricingLines,
        Money CustomerTotal,
        CandidateFareConstruction FareConstruction)
    {
        public const string CurrentSchemaVersion = "2.0";
    }
}
