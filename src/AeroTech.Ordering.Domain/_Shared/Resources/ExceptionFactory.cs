using AeroTech.Framework.Core.Domain.Exceptions;

namespace AeroTech.Ordering.Domain._Shared.Resources
{
    public static class ExceptionFactory
    {
        // Home operator identity: 2710-2719
        public static BusinessException HomeOperatorNotProvisioned(params object?[] args) =>
            new(20090, ExceptionMessages.HomeOperatorNotProvisioned, args) { HttpStatus = 500 };

        public static BusinessException IdempotencyKeyRequired(params object?[] args) =>
            new(20264, ExceptionMessages.IdempotencyKeyRequired, args) { HttpStatus = 400 };

        public static BusinessException IdempotencyKeyConflict(params object?[] args) =>
            new(20265, ExceptionMessages.IdempotencyKeyConflict, args) { HttpStatus = 409 };

        public static BusinessException PreparationNotFound(params object?[] args) =>
            new(20266, ExceptionMessages.PreparationNotFound, args) { HttpStatus = 404 };

        public static BusinessException PreparationAlreadyConsumed(params object?[] args) =>
            new(20267, ExceptionMessages.PreparationAlreadyConsumed, args) { HttpStatus = 409 };

        public static BusinessException AcceptedDigestMismatch(params object?[] args) =>
            new(20268, ExceptionMessages.AcceptedDigestMismatch, args) { HttpStatus = 409 };

        public static BusinessException AcceptanceProfileNotPermitted(params object?[] args) =>
            new(20269, ExceptionMessages.AcceptanceProfileNotPermitted, args) { HttpStatus = 422 };

        public static BusinessException SourceValidityExpired(params object?[] args) =>
            new(20270, ExceptionMessages.SourceValidityExpired, args) { HttpStatus = 422 };

        public static BusinessException SourceValidityNotEstablished(params object?[] args) =>
            new(20271, ExceptionMessages.SourceValidityNotEstablished, args) { HttpStatus = 422 };

        public static BusinessException CandidateContractMismatch(params object?[] args) =>
            new(20272, ExceptionMessages.CandidateContractMismatch, args) { HttpStatus = 422 };

        public static BusinessException UnsupportedCapability(params object?[] args) =>
            new(20273, ExceptionMessages.UnsupportedCapability, args) { HttpStatus = 422 };

        public static BusinessException OfferNotFound(params object?[] args) =>
            new(20274, ExceptionMessages.OfferNotFound, args) { HttpStatus = 422 };

        public static BusinessException OfferSourceUnavailable(params object?[] args) =>
            new(20275, ExceptionMessages.OfferSourceUnavailable, args) { HttpStatus = 503 };

        public static BusinessException TravellerBindingInvalid(params object?[] args) =>
            new(20276, ExceptionMessages.TravellerBindingInvalid, args) { HttpStatus = 422 };

        public static BusinessException ContactInvalid(params object?[] args) =>
            new(20277, ExceptionMessages.ContactInvalid, args) { HttpStatus = 422 };

        public static BusinessException RepresentationOverflow(params object?[] args) =>
            new(20278, ExceptionMessages.RepresentationOverflow, args) { HttpStatus = 422 };

        public static BusinessException PricingRuleViolated(params object?[] args) =>
            new(20279, ExceptionMessages.PricingRuleViolated, args) { HttpStatus = 422 };

        public static BusinessException AllocationInvalid(params object?[] args) =>
            new(20280, ExceptionMessages.AllocationInvalid, args) { HttpStatus = 422 };

        public static BusinessException ReversalInvalid(params object?[] args) =>
            new(20281, ExceptionMessages.ReversalInvalid, args) { HttpStatus = 422 };

        public static BusinessException OrderNotFound(params object?[] args) =>
            new(20282, ExceptionMessages.OrderNotFound, args) { HttpStatus = 404 };

        public static BusinessException OperationNotFound(params object?[] args) =>
            new(20283, ExceptionMessages.OperationNotFound, args) { HttpStatus = 404 };

        public static BusinessException ProjectionRevisionLagging(params object?[] args) =>
            new(20284, ExceptionMessages.ProjectionRevisionLagging, args) { HttpStatus = 503 };

        public static BusinessException AuthorizedScopeRequired(params object?[] args) =>
            new(20285, ExceptionMessages.AuthorizedScopeRequired, args) { HttpStatus = 403 };

        public static BusinessException SalesContextMismatch(params object?[] args) =>
            new(20287, ExceptionMessages.SalesContextMismatch, args) { HttpStatus = 409 };

        public static BusinessException ProjectionRebuildConflict(params object?[] args) =>
            new(20286, ExceptionMessages.ProjectionRebuildConflict, args) { HttpStatus = 409 };

        public static BusinessException ServiceCoverageIsNotASingleSegment(params object?[] args) =>
            new(20288, ExceptionMessages.ServiceCoverageIsNotASingleSegment, args) { HttpStatus = 422 };

        public static BusinessException SalesContextIncomplete(params object?[] args) =>
            new(20289, ExceptionMessages.SalesContextIncomplete, args) { HttpStatus = 422 };

        public static BusinessException SettlementAttributionIncomplete(params object?[] args) =>
            new(20290, ExceptionMessages.SettlementAttributionIncomplete, args) { HttpStatus = 422 };

        public static BusinessException SettlementAttributionNotAllowed(params object?[] args) =>
            new(20291, ExceptionMessages.SettlementAttributionNotAllowed, args) { HttpStatus = 422 };

        public static BusinessException SettlementAttributionRequired(params object?[] args) =>
            new(20292, ExceptionMessages.SettlementAttributionRequired, args) { HttpStatus = 422 };

        public static BusinessException AirServiceScopeInvalid(params object?[] args) =>
            new(20293, ExceptionMessages.AirServiceScopeInvalid, args) { HttpStatus = 422 };

        public static BusinessException ComponentTotalInvalid(params object[] args) =>
            new(20294, ExceptionMessages.ComponentTotalInvalid, args) { HttpStatus = 422 };

        public static BusinessException FundingObligationScopeInvalid(params object[] args) =>
            new(20295, ExceptionMessages.FundingObligationScopeInvalid, args) { HttpStatus = 422 };

        public static BusinessException FundingCoverageIncomplete(params object[] args) =>
            new(20296, ExceptionMessages.FundingCoverageIncomplete, args) { HttpStatus = 422 };
    }
}
