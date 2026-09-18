using AeroTech.Ordering.Domain._Shared.Resources;

namespace AeroTech.Ordering.Domain._Shared.ValueObjects
{
    public sealed record ProductSnapshot
    {
        public ProductSnapshot(
            string sourceSystem,
            string sourceOfferId,
            string? sourceOfferItemRef,
            string? productCode,
            string? productName,
            string? brandCode,
            string? brandName,
            string? productVersion)
        {
            if (string.IsNullOrWhiteSpace(sourceSystem))
                throw ExceptionFactory.CandidateContractMismatch("a product snapshot requires its source system");

            if (string.IsNullOrWhiteSpace(sourceOfferId))
                throw ExceptionFactory.CandidateContractMismatch("a product snapshot requires its source offer");

            SourceSystem = sourceSystem;
            SourceOfferId = sourceOfferId;
            SourceOfferItemRef = Trimmed(sourceOfferItemRef);
            ProductCode = Trimmed(productCode);
            ProductName = Trimmed(productName);
            BrandCode = Trimmed(brandCode);
            BrandName = Trimmed(brandName);
            ProductVersion = Trimmed(productVersion);
        }

        public string SourceSystem { get; }

        public string SourceOfferId { get; }

        public string? SourceOfferItemRef { get; }

        public string? ProductCode { get; }

        public string? ProductName { get; }

        public string? BrandCode { get; }

        public string? BrandName { get; }

        public string? ProductVersion { get; }

        private static string? Trimmed(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
