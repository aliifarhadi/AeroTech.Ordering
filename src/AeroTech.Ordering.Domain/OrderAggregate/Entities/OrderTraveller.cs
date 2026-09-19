using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Ordering.Domain.OrderAggregate.ValueObjects;
using AeroTech.Messages.Ordering.Enums;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class OrderTraveller : Entity<long>
    {
        private OrderTraveller()
        {
        }

        internal OrderTraveller(long id, long orderId, TravellerBinding binding)
        {
            Id = id;
            OrderId = orderId;
            SourceTravellerRef = binding.SourceTravellerRef;
            ClientTravellerRef = binding.ClientTravellerRef;
            PassengerTypeCode = binding.PassengerTypeCode;
            Identity = new TravellerIdentity(binding.GivenName, binding.Surname, binding.DateOfBirth);
        }

        public long OrderId { get; private set; }

        public string SourceTravellerRef { get; private set; } = null!;

        public string ClientTravellerRef { get; private set; } = null!;

        public PassengerTypeCode PassengerTypeCode { get; private set; }

        public long? InfantParentTravellerId { get; private set; }

        public TravellerIdentity Identity { get; private set; } = null!;

        internal void LinkGuardian(long guardianTravellerId) => InfantParentTravellerId = guardianTravellerId;
    }
}
