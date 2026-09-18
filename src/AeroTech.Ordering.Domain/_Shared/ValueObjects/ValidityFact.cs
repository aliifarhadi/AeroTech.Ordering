using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;

namespace AeroTech.Ordering.Domain._Shared.ValueObjects
{
    public sealed record ValidityFact
    {
        public ValidityFact(ValidityState state, DateTimeOffset? value, string owner, string? sourceRef, string? reason)
        {
            if (!Enum.IsDefined(state))
                throw ExceptionFactory.CandidateContractMismatch("validity state is not defined");

            if (string.IsNullOrWhiteSpace(owner))
                throw ExceptionFactory.CandidateContractMismatch("validity owner is required");

            if (state == ValidityState.Known && value is null)
                throw ExceptionFactory.CandidateContractMismatch("known validity requires a value");

            if (state != ValidityState.Known && value is not null)
                throw ExceptionFactory.CandidateContractMismatch("validity that is not known cannot carry a value");

            State = state;
            Value = value;
            Owner = owner;
            SourceRef = string.IsNullOrWhiteSpace(sourceRef) ? null : sourceRef;
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason;
        }

        public ValidityState State { get; }

        public DateTimeOffset? Value { get; }

        public string Owner { get; }

        public string? SourceRef { get; }

        public string? Reason { get; }

        public bool IsExpiredAt(DateTimeOffset now) => State == ValidityState.Known && now >= Value!.Value;
    }
}
