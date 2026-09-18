using AeroTech.Ordering.Domain._Shared.Resources;

namespace AeroTech.Ordering.Domain._Shared.ValueObjects
{
    public sealed record Money
    {
        public Money(decimal amount, string currencyRef)
        {
            if (string.IsNullOrWhiteSpace(currencyRef))
                throw ExceptionFactory.CandidateContractMismatch("money requires a currency reference");

            Amount = amount;
            CurrencyRef = currencyRef;
        }

        public decimal Amount { get; }

        public string CurrencyRef { get; }

        public bool IsNegative => Amount < 0;

        public bool SameCurrencyAs(Money other) => string.Equals(CurrencyRef, other.CurrencyRef, StringComparison.Ordinal);
    }
}
