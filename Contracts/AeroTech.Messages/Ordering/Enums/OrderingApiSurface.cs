using System.ComponentModel.DataAnnotations;

namespace AeroTech.Messages.Ordering.Enums
{
    public enum OrderingApiSurface
    {
        [Display(Name = "Service")] Service = 1,

        [Display(Name = "Back office")] Backoffice = 2,

        [Display(Name = "Ota Panel")] OtaPanel = 3,

        [Display(Name = "Ota")] Ota = 4,

        [Display(Name = "Internal")] Internal = 5
    }
}
