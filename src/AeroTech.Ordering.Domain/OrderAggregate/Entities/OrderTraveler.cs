using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Ordering.Domain.OrderAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class OrderTraveler : Entity<long>
    {
        private OrderTraveler()
        {
        }

        internal OrderTraveler(long id, long orderId, TravelerBinding binding)
        {
            Id = id;
            OrderId = orderId;
            SourceTravellerRef = binding.SourceTravellerRef;
            ClientTravelerRef = binding.ClientTravelerRef;
            PassengerTypeCode = binding.PassengerTypeCode;
            Identity = new TravelerIdentity(binding.GivenName, binding.Surname, binding.DateOfBirth);
        }

        public long OrderId { get; private set; }

        public string SourceTravellerRef { get; private set; } = null!;

        public string ClientTravelerRef { get; private set; } = null!;

        public string PassengerTypeCode { get; private set; } = null!;

        public long? InfantParentTravelerId { get; private set; }

        public TravelerIdentity Identity { get; private set; } = null!;

        internal void LinkGuardian(long guardianTravelerId) => InfantParentTravelerId = guardianTravelerId;
    }
}
