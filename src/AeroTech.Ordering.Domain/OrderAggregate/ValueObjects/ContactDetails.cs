using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Domain.OrderAggregate.ValueObjects
{
    public sealed record ContactDetails(ContactRole Role, string? Email, string? Phone);
}
