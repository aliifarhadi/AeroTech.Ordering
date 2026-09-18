using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Policies
{
    public static class PricingReversalPolicy
    {
        public static void EnsureReversible(
            ReversibleLine original,
            ReversibleLine reversal,
            IEnumerable<ReversibleLine> priorReversals)
        {
            if (reversal.Role != PricingLineRole.Reversal)
                throw ExceptionFactory.ReversalInvalid("the reversing line must have role Reversal");

            if (original.Role == PricingLineRole.Reversal)
                throw ExceptionFactory.ReversalInvalid("a reversal is undone by a named correction, not by reversing it");

            if (reversal.Direction == original.Direction)
                throw ExceptionFactory.ReversalInvalid("a reversal must use the opposite direction");

            if (reversal.Component != original.Component || reversal.Effect != original.Effect)
                throw ExceptionFactory.ReversalInvalid("a reversal must keep the original component and effect");

            if (!reversal.OriginalValue.SameCurrencyAs(original.OriginalValue) || !reversal.SaleValue.SameCurrencyAs(original.SaleValue))
                throw ExceptionFactory.ReversalInvalid("a reversal must keep the original currencies");

            PricingArithmetic.EnsureMagnitude(reversal.OriginalValue);
            PricingArithmetic.EnsureMagnitude(reversal.SaleValue);

            var prior = priorReversals.ToList();
            var reversedOriginal = prior.Sum(line => line.OriginalValue.Amount) + reversal.OriginalValue.Amount;
            var reversedSale = prior.Sum(line => line.SaleValue.Amount) + reversal.SaleValue.Amount;

            if (reversedOriginal > original.OriginalValue.Amount || reversedSale > original.SaleValue.Amount)
                throw ExceptionFactory.ReversalInvalid("cumulative reversal exceeds the still-reversible original value");
        }
    }
}
