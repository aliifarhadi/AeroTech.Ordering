using System.Text.Json;
using AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrder;

namespace AeroTech.Ordering.RestApi.V1.OrderAggregate.Responses
{
    public sealed record OrderDetailsResponse(
        long OrderId,
        long OrderRevision,
        int ProjectionSchemaVersion,
        DateTimeOffset ProjectedAt,
        JsonElement Details,
        IReadOnlyList<ProtectedTravelerResponse> ProtectedTravelers,
        IReadOnlyList<ProtectedContactResponse> ProtectedContacts)
    {
        public static OrderDetailsResponse From(OrderDetailsView view) => new(
            view.OrderId,
            view.OrderRevision,
            view.ProjectionSchemaVersion,
            view.ProjectedAt,
            view.Details,
            view.ProtectedTravelers
                .Select(traveler => new ProtectedTravelerResponse(
                    traveler.TravelerId,
                    traveler.SourceTravellerRef,
                    traveler.ClientTravelerRef,
                    traveler.PassengerTypeCode,
                    traveler.GivenName,
                    traveler.Surname,
                    traveler.DateOfBirth,
                    traveler.InfantParentTravelerId))
                .ToList(),
            view.ProtectedContacts
                .Select(contact => new ProtectedContactResponse(contact.ContactId, contact.Sequence, contact.Role, contact.Email, contact.Phone))
                .ToList());
    }
}
