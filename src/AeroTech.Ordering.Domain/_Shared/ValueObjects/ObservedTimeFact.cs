using AeroTech.Ordering.Domain._Shared.Resources;

namespace AeroTech.Ordering.Domain._Shared.ValueObjects
{
    public sealed record ObservedTimeFact
    {
        public ObservedTimeFact(DateTimeOffset value, string sourceOwner, string sourceRef)
        {
            if (string.IsNullOrWhiteSpace(sourceOwner))
                throw ExceptionFactory.CandidateContractMismatch("an observed time fact requires its source owner");

            if (string.IsNullOrWhiteSpace(sourceRef))
                throw ExceptionFactory.CandidateContractMismatch("an observed time fact requires its source reference");

            Value = value;
            SourceOwner = sourceOwner;
            SourceRef = sourceRef;
        }

        public DateTimeOffset Value { get; }

        public string SourceOwner { get; }

        public string SourceRef { get; }
    }
}
