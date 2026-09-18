using System.ComponentModel.DataAnnotations;

namespace AeroTech.Ordering.RestApi.V1.OrderAggregate.Requests
{
    public sealed class ServiceCreateOrderRequest : CreateOrderRequest
    {
        [Required]
        public long? FinancialCustomerId { get; set; }

        public long? SellingOfficeId { get; set; }
    }
}
