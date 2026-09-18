using AeroTech.Messages.Shared.Enums;
namespace AeroTech.Ordering.Providers.Deterministic.Offers.Persistence
{
    public sealed class ReferenceOfferRecord
    {
        public long Id { get; set; }

        public string OfferId { get; set; } = null!;

        public int Revision { get; set; }

        public string OwnerBindingRef { get; set; } = null!;

        public long OwnerAirlineId { get; set; }

        public long FinancialCustomerId { get; set; }

        public SalesChannel Channel { get; set; }

        public long? SellingOfficeId { get; set; }

        public string CandidateJson { get; set; } = null!;

        public bool IsCurrent { get; set; }

        public DateTimeOffset PublishedAt { get; set; }
    }
}
