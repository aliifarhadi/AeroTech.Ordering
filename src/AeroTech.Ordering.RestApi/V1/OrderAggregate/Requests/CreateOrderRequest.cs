using System.ComponentModel.DataAnnotations;

namespace AeroTech.Ordering.RestApi.V1.OrderAggregate.Requests
{
    public class CreateOrderRequest
    {
        [Required]
        public long? PreparationId { get; set; }

        [Required]
        [RegularExpression("^[a-f0-9]{64}$")]
        public string AcceptedSnapshotDigest { get; set; } = default!;

        [Required]
        public DateTimeOffset? AcceptedAt { get; set; }

        [Required]
        [MinLength(1)]
        public IReadOnlyList<TravelerBindingRequest> TravelerBindings { get; set; } = [];

        [Required]
        public IReadOnlyList<ContactRequest> Contacts { get; set; } = [];

        public string? ClientReference { get; set; }
    }
}
