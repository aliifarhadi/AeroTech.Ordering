using AeroTech.Ordering.Domain._Shared.Resources;

namespace AeroTech.Ordering.Domain._Shared.ValueObjects
{
    public sealed record AppliedConversion
    {
        public AppliedConversion(
            string sourceConversionRef,
            int fromCurrencyId,
            int toCurrencyId,
            decimal rate,
            int decimalPlaces,
            string? roundingToken)
        {
            if (string.IsNullOrWhiteSpace(sourceConversionRef))
                throw ExceptionFactory.CandidateContractMismatch("an applied conversion requires its source reference");

            if (fromCurrencyId <= 0 || toCurrencyId <= 0)
                throw ExceptionFactory.CandidateContractMismatch("an applied conversion requires both currency identifiers");

            if (rate <= 0m)
                throw ExceptionFactory.CandidateContractMismatch("an applied conversion rate must be positive");

            if (decimalPlaces < 0)
                throw ExceptionFactory.CandidateContractMismatch("an applied conversion cannot have negative decimal places");

            SourceConversionRef = sourceConversionRef;
            FromCurrencyId = fromCurrencyId;
            ToCurrencyId = toCurrencyId;
            Rate = rate;
            DecimalPlaces = decimalPlaces;
            RoundingToken = string.IsNullOrWhiteSpace(roundingToken) ? null : roundingToken;
        }

        public string SourceConversionRef { get; }

        public int FromCurrencyId { get; }

        public int ToCurrencyId { get; }

        public decimal Rate { get; }

        public int DecimalPlaces { get; }

        public string? RoundingToken { get; }
    }
}
