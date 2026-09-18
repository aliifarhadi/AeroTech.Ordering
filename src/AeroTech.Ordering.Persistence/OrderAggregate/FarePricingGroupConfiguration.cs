using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class FarePricingGroupConfiguration : IEntityTypeConfiguration<FarePricingGroup>
    {
        public void Configure(EntityTypeBuilder<FarePricingGroup> builder)
        {
            builder.ToTable("FarePricingGroups", PersistenceSchemas.Order, table =>
                table.HasCheckConstraint("CK_FarePricingGroups_Quantity", "[Quantity] >= 1"));
            builder.HasKey(group => group.Id);
            builder.Property(group => group.Id).ValueGeneratedNever();
            builder.HasMany(group => group.Travelers).WithOne().HasForeignKey(traveler => traveler.PricingGroupId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(group => group.Travelers).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
