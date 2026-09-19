using System.ComponentModel.DataAnnotations;

namespace AeroTech.Messages.Ordering.Enums
{
    public enum SellingOfficeKind
    {
        [Display(Name = "NotRecorded")] NotRecorded = 0,

        [Display(Name = "AirlineOffice")] AirlineOffice = 1,

        [Display(Name = "TravelAgencyOffice")] TravelAgencyOffice = 2
    }
}
