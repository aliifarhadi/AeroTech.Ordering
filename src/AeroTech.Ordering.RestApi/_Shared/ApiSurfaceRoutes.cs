namespace AeroTech.Ordering.RestApi._Shared
{
    public static class ApiSurfaceRoutes
    {
        public const string Service = "service/v{version:apiVersion}";
        public const string Backoffice = "backoffice/v{version:apiVersion}";
        public const string OtaPanel = "otapanel/v{version:apiVersion}";
        public const string Ota = "ota/v{version:apiVersion}";
        public const string Internal = "internal/v{version:apiVersion}";

        public const string OrderPreparations = "order-preparations";
        public const string OrdersFromOffer = "orders/from-offer";
        public const string Orders = "orders";
        public const string Operations = "operations";
        public const string ProjectionRebuilds = "orders/{orderId}/projection-rebuilds";
    }
}
