using AeroTech.Messages.Aegis.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;

namespace AeroTech.Ordering.Domain._Shared.ValueObjects
{
    public sealed record BuyerSnapshot
    {
        public BuyerSnapshot(BusinessContextType? contextType, long? buyerId)
        {
            if (contextType is null != buyerId is null)
                throw ExceptionFactory.SalesContextIncomplete("buyer context type and buyer identifier");

            if (buyerId is <= 0)
                throw ExceptionFactory.SalesContextIncomplete("buyer identifier");

            ContextType = contextType;
            BuyerId = buyerId;
        }

        public static BuyerSnapshot NotSupplied { get; } = new(null, null);

        public BusinessContextType? ContextType { get; }

        public long? BuyerId { get; }

        public bool IsSupplied => BuyerId is not null;
    }
}
