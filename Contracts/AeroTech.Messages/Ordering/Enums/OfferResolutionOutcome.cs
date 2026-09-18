using System.ComponentModel.DataAnnotations;

namespace AeroTech.Messages.Ordering.Enums
{
    public enum OfferResolutionOutcome
    {
        [Display(Name = "Resolved")] Resolved = 1,
        [Display(Name = "Not Found")] NotFound = 2,
        [Display(Name = "Contract Mismatch")] ContractMismatch = 3,
        [Display(Name = "Unsupported Capability")] UnsupportedCapability = 4,
        [Display(Name = "Unavailable")] Unavailable = 5,
    }
}
