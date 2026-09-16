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
    }
}
