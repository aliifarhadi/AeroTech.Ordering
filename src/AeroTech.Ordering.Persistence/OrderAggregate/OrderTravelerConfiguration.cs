using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class OrderTravelerConfiguration : IEntityTypeConfiguration<OrderTraveler>
    {
        public void Configure(EntityTypeBuilder<OrderTraveler> builder)
        {
            builder.ToTable("OrderTravelers", PersistenceSchemas.Order, table =>
                table.HasCheckConstraint("CK_OrderTravelers_Guardian", "[InfantParentTravelerId] IS NULL OR [InfantParentTravelerId] <> [Id]"));
            builder.HasKey(traveler => traveler.Id);
            builder.Property(traveler => traveler.Id).ValueGeneratedNever();
            builder.Property(traveler => traveler.SourceTravellerRef).HasMaxLength(PersistenceSchemas.ReferenceLength).IsRequired();
            builder.Property(traveler => traveler.ClientTravelerRef).HasMaxLength(PersistenceSchemas.ReferenceLength).IsRequired();

            builder.OwnsOne(traveler => traveler.Identity, identity =>
            {
                identity.ToTable("OrderTravelerIdentities", PersistenceSchemas.Order);
                identity.WithOwner().HasForeignKey("TravelerId");
                identity.Property(value => value.GivenName).HasMaxLength(PersistenceSchemas.PersonNameLength).IsRequired();
                identity.Property(value => value.Surname).HasMaxLength(PersistenceSchemas.PersonNameLength).IsRequired();
                identity.Property(value => value.DateOfBirth).IsRequired();
            });

            builder.HasOne<OrderTraveler>().WithMany().HasForeignKey(traveler => traveler.InfantParentTravelerId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(traveler => new { traveler.OrderId, traveler.SourceTravellerRef }).IsUnique();
            builder.HasIndex(traveler => new { traveler.OrderId, traveler.ClientTravelerRef }).IsUnique();
        }
    }
}
