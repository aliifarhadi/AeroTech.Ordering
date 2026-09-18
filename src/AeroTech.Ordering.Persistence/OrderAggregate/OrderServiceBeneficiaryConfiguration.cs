using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class OrderServiceBeneficiaryConfiguration : IEntityTypeConfiguration<OrderServiceBeneficiary>
    {
        public void Configure(EntityTypeBuilder<OrderServiceBeneficiary> builder)
        {
            builder.ToTable("OrderServiceBeneficiaries", PersistenceSchemas.Order);
            builder.HasKey(beneficiary => beneficiary.Id);
            builder.Property(beneficiary => beneficiary.Id).ValueGeneratedNever();
            builder.HasOne<OrderTraveler>().WithMany().HasForeignKey(beneficiary => beneficiary.TravelerId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(beneficiary => new { beneficiary.ServiceId, beneficiary.TravelerId }).IsUnique();
        }
    }
}
