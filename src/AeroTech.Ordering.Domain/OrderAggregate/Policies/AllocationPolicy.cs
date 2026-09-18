using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;
using AeroTech.Ordering.Domain._Shared.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Policies
{
    public static class AllocationPolicy
    {
        public static void EnsureConsistent(Money parentOriginal, Money parentSale, AllocationProposal proposal)
        {
            if (proposal.Version < 1)
                throw ExceptionFactory.AllocationInvalid("version starts at 1");

            if (!Enum.IsDefined(proposal.Purpose) || !Enum.IsDefined(proposal.Method) || !Enum.IsDefined(proposal.Completeness))
                throw ExceptionFactory.AllocationInvalid("purpose, method and completeness must be defined");

            if (proposal.Completeness == PricingAllocationCompleteness.Unavailable)
            {
                if (proposal.Rows.Count > 0)
                    throw ExceptionFactory.AllocationInvalid("an unavailable allocation has no rows");

                return;
            }

            if (proposal.Rows.Count == 0)
                throw ExceptionFactory.AllocationInvalid("a complete or partial allocation needs rows");

            foreach (var row in proposal.Rows)
            {
                if (!row.OriginalValue.SameCurrencyAs(parentOriginal) || !row.SaleValue.SameCurrencyAs(parentSale))
                    throw ExceptionFactory.AllocationInvalid("rows keep the parent line currencies");

                if (row.OriginalValue.IsNegative || row.SaleValue.IsNegative)
                    throw ExceptionFactory.AllocationInvalid("rows are nonnegative shares");
            }

            var original = proposal.Rows.Sum(row => row.OriginalValue.Amount);
            var sale = proposal.Rows.Sum(row => row.SaleValue.Amount);

            if (proposal.Completeness == PricingAllocationCompleteness.Complete
                && (original != parentOriginal.Amount || sale != parentSale.Amount))
                throw ExceptionFactory.AllocationInvalid("a complete allocation sums exactly to the parent line");

            if (proposal.Completeness == PricingAllocationCompleteness.Partial
                && (original > parentOriginal.Amount || sale > parentSale.Amount))
                throw ExceptionFactory.AllocationInvalid("a partial allocation cannot exceed the parent line");
        }

        public static AllocationProposal Select(
            IEnumerable<AllocationProposal> sets,
            PricingAllocationPurpose purpose,
            int version)
            => sets.SingleOrDefault(set => set.Purpose == purpose && set.Version == version)
               ?? throw ExceptionFactory.AllocationInvalid($"no {purpose} allocation version {version}");
    }
}
