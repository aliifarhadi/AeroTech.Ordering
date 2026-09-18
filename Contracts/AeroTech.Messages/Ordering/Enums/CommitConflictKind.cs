using System.ComponentModel.DataAnnotations;

namespace AeroTech.Messages.Ordering.Enums
{
    public enum CommitConflictKind
    {
        [Display(Name = "Command Receipt Key")] CommandReceiptKey = 1,
        [Display(Name = "Preparation Consumption")] PreparationConsumption = 2,
        [Display(Name = "Order Reference")] OrderReference = 3,
        [Display(Name = "Projection Revision")] ProjectionRevision = 4,
    }
}
