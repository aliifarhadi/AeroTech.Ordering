using AeroTech.Ordering.Domain._Shared.Resources;

namespace AeroTech.Ordering.Domain._Shared.ValueObjects
{
    public sealed record Money
    {
        public Money(decimal amount, int currencyId)
        {
            if (currencyId <= 0)
                throw ExceptionFactory.CandidateContractMismatch("money requires a currency identifier");

            Amount = amount;
            CurrencyId = currencyId;
        }

        public decimal Amount { get; }

        public int CurrencyId { get; }

        public bool IsNegative => Amount < 0;

        public bool SameCurrencyAs(Money other) => CurrencyId == other.CurrencyId;
    }
}
