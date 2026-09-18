namespace AeroTech.Ordering.Persistence._Shared.Mapping
{
    public static class PersistenceSchemas
    {
        public const string Order = "Order";
        public const string Operations = "Operations";

        public const int ReferenceLength = 128;
        public const int CurrencyRefLength = 32;

        public const int CurrencyCodeLength = 8;
        public const int DigestLength = 64;
        public const int OwnerNameLength = 64;
        public const int ProfileLength = 64;
        public const int ReasonLength = 512;
        public const int IdempotencyKeyLength = 128;
        public const int CallerScopeLength = 256;
        public const int PassengerTypeLength = 8;
        public const int PersonNameLength = 128;
        public const int EmailLength = 256;
        public const int PhoneLength = 64;
    }
}
