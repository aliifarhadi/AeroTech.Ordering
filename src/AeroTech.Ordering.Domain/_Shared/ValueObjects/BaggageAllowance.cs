using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain._Shared.Resources;

namespace AeroTech.Ordering.Domain._Shared.ValueObjects
{
    public sealed record BaggageAllowance
    {
        public BaggageAllowance(int? pieces, decimal? weight, BaggageWeightUnit? weightUnit)
        {
            if (pieces is < 0)
                throw ExceptionFactory.CandidateContractMismatch("a baggage allowance cannot have negative pieces");

            if (weight is < 0m)
                throw ExceptionFactory.CandidateContractMismatch("a baggage allowance cannot have negative weight");

            if (weight is not null && weightUnit is null)
                throw ExceptionFactory.CandidateContractMismatch("a baggage allowance weight requires its unit");

            if (weight is null && weightUnit is not null)
                throw ExceptionFactory.CandidateContractMismatch("a baggage allowance unit requires a weight");

            if (pieces is null && weight is null)
                throw ExceptionFactory.CandidateContractMismatch("a baggage allowance requires pieces or weight");

            Pieces = pieces;
            Weight = weight;
            WeightUnit = weightUnit;
        }

        public int? Pieces { get; }

        public decimal? Weight { get; }

        public BaggageWeightUnit? WeightUnit { get; }
    }
}
