using AeroTech.Framework.Core.Domain.Aggregates;
using AeroTech.Ordering.Domain._Shared.Resources;

namespace AeroTech.Ordering.Domain.CommandReceiptAggregate
{
    public sealed class CommandReceipt : AggregateRoot<long>
    {
        private CommandReceipt()
        {
        }

        public long OwnerAirlineId { get; private set; }

        public long FinancialCustomerId { get; private set; }

        public string CallerScope { get; private set; } = null!;

        public OrderingCommandKind CommandKind { get; private set; }

        public string IdempotencyKey { get; private set; } = null!;

        public string CanonicalizationVersion { get; private set; } = null!;

        public string RequestDigest { get; private set; } = null!;

        public long? OrderId { get; private set; }

        public string ResultJson { get; private set; } = null!;

        public DateTimeOffset CreatedAt { get; private set; }

        public static CommandReceipt Completed(
            long id,
            ReceiptScope scope,
            string canonicalizationVersion,
            string requestDigest,
            long? orderId,
            string resultJson,
            DateTimeOffset now)
        {
            if (string.IsNullOrWhiteSpace(scope.IdempotencyKey))
                throw ExceptionFactory.IdempotencyKeyRequired("Idempotency-Key");

            return new CommandReceipt
            {
                Id = id,
                OwnerAirlineId = scope.OwnerAirlineId,
                FinancialCustomerId = scope.FinancialCustomerId,
                CallerScope = scope.CallerScope,
                CommandKind = scope.CommandKind,
                IdempotencyKey = scope.IdempotencyKey,
                CanonicalizationVersion = canonicalizationVersion,
                RequestDigest = requestDigest,
                OrderId = orderId,
                ResultJson = resultJson,
                CreatedAt = now
            };
        }

        public bool Matches(string requestDigest, long financialCustomerId)
            => FinancialCustomerId == financialCustomerId && string.Equals(RequestDigest, requestDigest, StringComparison.Ordinal);

        public void EnsureSameRequest(string requestDigest, long financialCustomerId)
        {
            if (!Matches(requestDigest, financialCustomerId))
                throw ExceptionFactory.IdempotencyKeyConflict(IdempotencyKey, CommandKind);
        }
    }
}
