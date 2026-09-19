using AeroTech.Ordering.Domain._Shared.Resources;

namespace AeroTech.Ordering.Domain._Shared.ValueObjects
{
    public sealed record SettlementAttribution
    {
        public SettlementAttribution(string partyRef, string categoryCode)
        {
            if (string.IsNullOrWhiteSpace(partyRef))
                throw ExceptionFactory.SettlementAttributionIncomplete("settlement party reference");

            if (string.IsNullOrWhiteSpace(categoryCode))
                throw ExceptionFactory.SettlementAttributionIncomplete("settlement category code");

            PartyRef = partyRef;
            CategoryCode = categoryCode;
        }

        public string PartyRef { get; }

        public string CategoryCode { get; }
    }
}
