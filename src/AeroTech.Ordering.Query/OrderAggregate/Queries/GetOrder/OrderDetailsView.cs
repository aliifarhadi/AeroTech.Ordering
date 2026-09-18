using System.Text.Json;

namespace AeroTech.Ordering.Query.OrderAggregate.Queries.GetOrder
{
    public sealed record OrderDetailsView(
        long OrderId,
        long OrderRevision,
        int ProjectionSchemaVersion,
        DateTimeOffset ProjectedAt,
        JsonElement Details,
        IReadOnlyList<ProtectedTravelerView> ProtectedTravelers,
        IReadOnlyList<ProtectedContactView> ProtectedContacts);
}
