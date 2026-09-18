using System.Globalization;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;

namespace AeroTech.Ordering.Application._Shared.Authorization
{
    public static class CallerScopeKey
    {
        public const string None = "none";

        public static string ForSale(OrderingApiSurface surface, long financialCustomerId, long? sellingOfficeId, string principalScope)
            => string.Join('|',
                Surface(surface),
                $"customer:{Id(financialCustomerId)}",
                $"office:{(sellingOfficeId is { } office ? Id(office) : None)}",
                $"principal:{principalScope}");

        public static string ForAdministration(OrderingApiSurface surface, string principalScope)
            => string.Join('|', Surface(surface), $"principal:{principalScope}");

        public static string Agency(long travelAgencyId) => $"agency:{Id(travelAgencyId)}";

        public static string Partner(long partnerApiAccessProfileId) => $"partner:{Id(partnerApiAccessProfileId)}";

        public static string Surface(OrderingApiSurface surface) => surface switch
        {
            OrderingApiSurface.Service => "service",
            OrderingApiSurface.Backoffice => "backoffice",
            OrderingApiSurface.OtaPanel => "otapanel",
            OrderingApiSurface.Ota => "ota",
            OrderingApiSurface.Internal => "internal",
            _ => throw ExceptionFactory.AuthorizedScopeRequired("an unknown api surface")
        };

        private static string Id(long value) => value.ToString(CultureInfo.InvariantCulture);
    }
}
