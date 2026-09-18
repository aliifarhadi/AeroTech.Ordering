using System.ComponentModel.DataAnnotations;

namespace AeroTech.Ordering.RestApi.V1.OrderAggregate.Requests
{
    public sealed class BackofficeCreateOrderRequest : CreateOrderRequest
    {
        [Required]
        public long? FinancialCustomerId { get; set; }

        [Required]
        public long? SellingOfficeId { get; set; }
    }
}
