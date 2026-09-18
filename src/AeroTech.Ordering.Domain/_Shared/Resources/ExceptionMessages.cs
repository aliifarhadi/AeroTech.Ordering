namespace AeroTech.Ordering.Domain._Shared.Resources
{
    public static class ExceptionMessages
    {
        // Home operator identity
        public const string HomeOperatorNotProvisioned = "The trusted home operator identity ('{0}') has not been synchronized from Core; the operation cannot establish its owning airline.";

        public const string IdempotencyKeyRequired = "The '{0}' header is required for this operation.";

        public const string IdempotencyKeyConflict = "Idempotency key '{0}' was already used for a different {1} request.";

        public const string PreparationNotFound = "Order preparation '{0}' was not found.";

        public const string PreparationAlreadyConsumed = "Order preparation '{0}' was already consumed by order '{1}'.";

        public const string AcceptedDigestMismatch = "The accepted snapshot digest does not match order preparation '{0}'.";

        public const string AcceptanceProfileNotPermitted = "Acceptance profile '{0}' is not permitted in this deployment ({1}).";

        public const string SourceValidityExpired = "The {0} validity supplied by {1} expired at {2:O}.";

        public const string SourceValidityNotEstablished = "The {0} validity is '{1}'; an owner-bound acceptance requires a known validity.";

        public const string CandidateContractMismatch = "The offer candidate violates the source contract: {0}.";

        public const string UnsupportedCapability = "Unsupported capability: {0}.";

        public const string OfferNotFound = "Offer '{0}' is not available from the offer source.";

        public const string OfferSourceUnavailable = "The offer source is unavailable: {0}.";

        public const string TravelerBindingInvalid = "Traveler binding is invalid: {0}.";

        public const string ContactInvalid = "Contact is invalid: {0}.";

        public const string RepresentationOverflow = "The value '{0}' of '{1}' exceeds the storage representation ({2},{3}).";

        public const string PricingRuleViolated = "Pricing rule violated: {0}.";

        public const string AllocationInvalid = "Allocation set is invalid: {0}.";

        public const string ReversalInvalid = "Reversal is invalid: {0}.";

        public const string OrderNotFound = "Order '{0}' was not found.";

        public const string OperationNotFound = "Operation '{0}' was not found.";

        public const string ProjectionRevisionLagging = "Order '{0}' projection is at revision {1}; revision {2} is not yet available.";

        public const string SalesContextMismatch = "Order preparation '{0}' was captured for a different sales context.";

        public const string AuthorizedScopeRequired = "The caller has no authorized scope for {0}.";

        public const string ProjectionRebuildConflict = "Order '{0}' changed during projection rebuild; retry.";
    }
}
