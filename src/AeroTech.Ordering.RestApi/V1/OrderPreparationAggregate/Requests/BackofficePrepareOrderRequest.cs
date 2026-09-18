using System.ComponentModel.DataAnnotations;

namespace AeroTech.Ordering.RestApi.V1.OrderPreparationAggregate.Requests
{
    public sealed class BackofficePrepareOrderRequest : PrepareOrderRequest
    {
        [Required]
        public long? FinancialCustomerId { get; set; }

        [Required]
        public long? SellingOfficeId { get; set; }
    }
}
