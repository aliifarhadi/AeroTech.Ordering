using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.RestApi.V1.OrderAggregate.Responses
{
    public sealed record ProtectedContactResponse(
        long ContactId,
        int Sequence,
        ContactRole Role,
        string? Email,
        string? Phone);
}
