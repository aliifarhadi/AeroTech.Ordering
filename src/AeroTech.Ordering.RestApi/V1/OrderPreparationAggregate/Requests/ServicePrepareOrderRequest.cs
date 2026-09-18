using System.ComponentModel.DataAnnotations;

namespace AeroTech.Ordering.RestApi.V1.OrderPreparationAggregate.Requests
{
    public sealed class ServicePrepareOrderRequest : PrepareOrderRequest
    {
        [Required]
        public long? FinancialCustomerId { get; set; }

        public long? SellingOfficeId { get; set; }
    }
}
