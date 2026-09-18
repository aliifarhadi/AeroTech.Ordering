using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderAggregate.ValueObjects;

namespace AeroTech.Ordering.Domain.OrderAggregate.Entities
{
    public sealed class OrderContact : Entity<long>
    {
        private OrderContact()
        {
        }

        internal OrderContact(long id, long orderId, int sequence, ContactDetails details)
        {
            Id = id;
            OrderId = orderId;
            Sequence = sequence;
            Role = details.Role;
            Email = details.Email;
            Phone = details.Phone;
        }

        public long OrderId { get; private set; }

        public int Sequence { get; private set; }

        public ContactRole Role { get; private set; }

        public string? Email { get; private set; }

        public string? Phone { get; private set; }
    }
}
