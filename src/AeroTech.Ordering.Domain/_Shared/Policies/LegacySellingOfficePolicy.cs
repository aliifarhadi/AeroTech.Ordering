using AeroTech.Messages.Ordering.Enums;
using AeroTech.Messages.Shared.Enums;

namespace AeroTech.Ordering.Domain._Shared.Policies
{
    public static class LegacySellingOfficePolicy
    {
        public static SellingOfficeKind? KindOf(SalesChannel channel, long? sellingOfficeId)
        {
            if (sellingOfficeId is null)
                return null;

            return channel switch
            {
                SalesChannel.BackOffice => SellingOfficeKind.AirlineOffice,
                SalesChannel.AgencyPanel => SellingOfficeKind.TravelAgencyOffice,
                SalesChannel.PartnerAPI => SellingOfficeKind.TravelAgencyOffice,
                _ => SellingOfficeKind.NotRecorded
            };
        }
    }
}
