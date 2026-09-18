using System.ComponentModel.DataAnnotations;

namespace AeroTech.Messages.Ordering.Enums
{
    public enum OrderingCommandKind
    {
        [Display(Name = "Prepare Order From Offer")] PrepareOrderFromOffer = 1,
        [Display(Name = "Create Order From Offer")] CreateOrderFromOffer = 2,
        [Display(Name = "Rebuild Order Projection")] RebuildOrderProjection = 3,
    }
}
