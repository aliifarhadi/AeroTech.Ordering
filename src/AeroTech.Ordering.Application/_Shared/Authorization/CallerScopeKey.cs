using System.Globalization;

namespace AeroTech.Ordering.Application._Shared.Authorization
{
    public static class CallerScopeKey
    {
        public const string Backoffice = "backoffice";
        public const string Ota = "ota";
        public const string OtaPanel = "otapanel";
        public const string Service = "service";
        public const string Internal = "internal";
        public const string None = "none";

        public static string ForSale(string surface, long financialCustomerId, long? sellingOfficeId, string principalScope)
            => string.Join('|',
                surface,
                $"customer:{Id(financialCustomerId)}",
                $"office:{(sellingOfficeId is { } office ? Id(office) : None)}",
                $"principal:{principalScope}");

        public static string ForAdministration(string surface) => string.Join('|', surface, $"principal:{None}");

        public static string Agency(long travelAgencyId) => $"agency:{Id(travelAgencyId)}";

        public static string Partner(long partnerApiAccessProfileId) => $"partner:{Id(partnerApiAccessProfileId)}";

        private static string Id(long value) => value.ToString(CultureInfo.InvariantCulture);
    }
}
