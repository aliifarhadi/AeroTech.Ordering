using AeroTech.Ordering.Domain._Shared.Resources;

namespace AeroTech.Ordering.Domain._Shared.ValueObjects
{
    public sealed record AppliedConversion
    {
        public AppliedConversion(
            string sourceConversionRef,
            string fromCurrencyRef,
            string toCurrencyRef,
            decimal rate,
            int decimalPlaces,
            string? roundingToken)
        {
            if (string.IsNullOrWhiteSpace(sourceConversionRef))
                throw ExceptionFactory.CandidateContractMismatch("an applied conversion requires its source reference");

            if (string.IsNullOrWhiteSpace(fromCurrencyRef) || string.IsNullOrWhiteSpace(toCurrencyRef))
                throw ExceptionFactory.CandidateContractMismatch("an applied conversion requires both currency references");

            if (rate <= 0m)
                throw ExceptionFactory.CandidateContractMismatch("an applied conversion rate must be positive");

            if (decimalPlaces < 0)
                throw ExceptionFactory.CandidateContractMismatch("an applied conversion cannot have negative decimal places");

            SourceConversionRef = sourceConversionRef;
            FromCurrencyRef = fromCurrencyRef;
            ToCurrencyRef = toCurrencyRef;
            Rate = rate;
            DecimalPlaces = decimalPlaces;
            RoundingToken = string.IsNullOrWhiteSpace(roundingToken) ? null : roundingToken;
        }

        public string SourceConversionRef { get; }

        public string FromCurrencyRef { get; }

        public string ToCurrencyRef { get; }

        public decimal Rate { get; }

        public int DecimalPlaces { get; }

        public string? RoundingToken { get; }
    }
}
