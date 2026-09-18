using System.ComponentModel.DataAnnotations;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderAggregate.ValueObjects;

namespace AeroTech.Ordering.RestApi.V1.OrderAggregate.Requests
{
    public sealed class ContactRequest
    {
        [Required]
        public ContactRole? Role { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Phone { get; set; }

        public ContactDetails ToDetails() => new(Role!.Value, Email, Phone);
    }
}
