using AeroTech.Messages.Aegis.Enums;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Messages.Shared.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;

namespace AeroTech.Ordering.Domain._Shared.ValueObjects
{
    public sealed record SalesContextSnapshot
    {
        public SalesContextSnapshot(
            SalesChannel channel,
            BusinessContextType? sellerContextType,
            long? sellerId,
            SellingOfficeKind? sellingOfficeKind,
            long? sellingOfficeId)
        {
            if (!Enum.IsDefined(channel))
                throw ExceptionFactory.SalesContextIncomplete("a defined sales channel");

            if (sellerContextType is { } seller && !Enum.IsDefined(seller))
                throw ExceptionFactory.SalesContextIncomplete("a defined seller context type");

            if (sellingOfficeKind is { } officeKind && !Enum.IsDefined(officeKind))
                throw ExceptionFactory.SalesContextIncomplete("a defined selling office kind");

            if (sellerContextType is null != sellerId is null)
                throw ExceptionFactory.SalesContextIncomplete("seller context type and seller identifier");

            if (sellingOfficeKind is null != sellingOfficeId is null)
                throw ExceptionFactory.SalesContextIncomplete("selling office kind and selling office identifier");

            if (sellerId is <= 0)
                throw ExceptionFactory.SalesContextIncomplete("seller identifier");

            if (sellingOfficeId is <= 0)
                throw ExceptionFactory.SalesContextIncomplete("selling office identifier");

            Channel = channel;
            SellerContextType = sellerContextType;
            SellerId = sellerId;
            SellingOfficeKind = sellingOfficeKind;
            SellingOfficeId = sellingOfficeId;
        }

        public SalesChannel Channel { get; }

        public BusinessContextType? SellerContextType { get; }

        public long? SellerId { get; }

        public SellingOfficeKind? SellingOfficeKind { get; }

        public long? SellingOfficeId { get; }

        public bool HasSeller => SellerId is not null;

        public bool HasSellingOffice => SellingOfficeId is not null;

        public static SalesContextSnapshot SellerNotSupplied(SalesChannel channel, SellingOfficeKind? kind, long? officeId)
            => new(channel, null, null, kind, officeId);
    }
}
