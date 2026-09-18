using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class OrderContactConfiguration : IEntityTypeConfiguration<OrderContact>
    {
        public void Configure(EntityTypeBuilder<OrderContact> builder)
        {
            builder.ToTable("OrderContacts", PersistenceSchemas.Order);
            builder.HasKey(contact => contact.Id);
            builder.Property(contact => contact.Id).ValueGeneratedNever();
            builder.Property(contact => contact.Email).HasMaxLength(PersistenceSchemas.EmailLength);
            builder.Property(contact => contact.Phone).HasMaxLength(PersistenceSchemas.PhoneLength);
            builder.HasIndex(contact => new { contact.OrderId, contact.Sequence }).IsUnique();
        }
    }
}
