using AeroTech.Ordering.Domain._Shared.Resources;

namespace AeroTech.Ordering.Domain._Shared.ValueObjects
{
    public sealed record CurrencySnapshot
    {
        public CurrencySnapshot(string currencyRef, string? currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyRef))
                throw ExceptionFactory.CandidateContractMismatch("a currency snapshot requires a currency reference");

            CurrencyRef = currencyRef;
            CurrencyCode = string.IsNullOrWhiteSpace(currencyCode) ? null : currencyCode;
        }

        public string CurrencyRef { get; }

        public string? CurrencyCode { get; }
    }
}
