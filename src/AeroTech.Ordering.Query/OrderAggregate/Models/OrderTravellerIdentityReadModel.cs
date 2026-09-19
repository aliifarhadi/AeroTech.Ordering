namespace AeroTech.Ordering.Query.OrderAggregate.Models
{
    public sealed class OrderTravellerIdentityReadModel
    {
        public long TravellerId { get; set; }

        public string GivenName { get; set; } = default!;

        public string Surname { get; set; } = default!;

        public DateOnly DateOfBirth { get; set; }
    }
}
