using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Application._Shared.Authorization;
using AeroTech.Ordering.Application.OrderAggregate.Commands.CreateOrderFromOffer;
using AeroTech.Ordering.Application.OrderPreparationAggregate.Commands.PrepareOrderFromOffer;
using AeroTech.Ordering.Domain.OrderAggregate.ValueObjects;
using AeroTech.Ordering.Domain.OrderPreparationAggregate.ValueObjects;

namespace AeroTech.Ordering.Persistence.Tests._Shared
{
    public static class S1Commands
    {
        public static string NewKey(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

        public static PrepareOrderFromOfferCommand Prepare(string offerId, SalesScopeRequest? scope = null, string? key = null)
            => new(scope ?? S1Harness.Sale(), key ?? NewKey("prepare"), offerId, null);

        public static CreateOrderFromOfferCommand Create(
            PreparationResult preparation,
            DateTimeOffset acceptedAt,
            SalesScopeRequest? scope = null,
            string? key = null,
            IReadOnlyList<TravelerBinding>? bindings = null)
            => new(
                scope ?? S1Harness.Sale(),
                key ?? NewKey("create"),
                preparation.PreparationId,
                preparation.AcceptedSnapshotDigest,
                acceptedAt,
                bindings ?? BindAll(preparation.Candidate),
                [new ContactDetails(ContactRole.Primary, "traveler@example.invalid", null)],
                null);

        public static IReadOnlyList<TravelerBinding> BindAll(NormalizedCandidate candidate)
            => candidate.Travelers
                .Select(traveler => new TravelerBinding(
                    traveler.SourceTravellerRef,
                    $"CLIENT-{traveler.SourceTravellerRef}",
                    "Sample",
                    "Traveler",
                    traveler.PassengerTypeCode,
                    new DateOnly(1990, 1, 1),
                    null))
                .ToList();
    }
}
