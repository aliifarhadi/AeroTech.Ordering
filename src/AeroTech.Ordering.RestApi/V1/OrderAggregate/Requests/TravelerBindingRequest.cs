using System.ComponentModel.DataAnnotations;
using AeroTech.Ordering.Domain.OrderAggregate.ValueObjects;

namespace AeroTech.Ordering.RestApi.V1.OrderAggregate.Requests
{
    public sealed class TravelerBindingRequest
    {
        [Required]
        [MinLength(1)]
        public string SourceTravellerRef { get; set; } = default!;

        [Required]
        [MinLength(1)]
        public string ClientTravelerRef { get; set; } = default!;

        [Required]
        [MinLength(1)]
        public string GivenName { get; set; } = default!;

        [Required]
        [MinLength(1)]
        public string Surname { get; set; } = default!;

        [Required]
        [MinLength(1)]
        public string PassengerTypeCode { get; set; } = default!;

        [Required]
        public DateOnly? DateOfBirth { get; set; }

        public string? GuardianClientTravelerRef { get; set; }

        public TravelerBinding ToBinding() => new(
            SourceTravellerRef,
            ClientTravelerRef,
            GivenName,
            Surname,
            PassengerTypeCode,
            DateOfBirth!.Value,
            GuardianClientTravelerRef);
    }
}
