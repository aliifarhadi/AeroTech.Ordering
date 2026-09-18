using System.ComponentModel.DataAnnotations;

namespace AeroTech.Ordering.RestApi.V1.OrderPreparationAggregate.Requests
{
    public class PrepareOrderRequest
    {
        [Required]
        [MinLength(1)]
        public string OfferId { get; set; } = default!;

        public string? ClientReference { get; set; }
    }
}
