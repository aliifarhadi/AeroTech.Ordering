namespace AeroTech.Ordering.Domain._Shared.Resources
{
    public static class ExceptionMessages
    {
        // Home operator identity
        public const string HomeOperatorNotProvisioned = "The trusted home operator identity ('{0}') has not been synchronized from Core; the operation cannot establish its owning airline.";

        public const string IdempotencyKeyRequired = "The '{0}' header is required for this operation.";
    }
}
