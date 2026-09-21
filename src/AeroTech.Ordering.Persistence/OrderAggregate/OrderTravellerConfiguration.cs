using AeroTech.Messages.Ordering.Enums;
using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class OrderTravellerConfiguration : IEntityTypeConfiguration<OrderTraveller>
    {
        public void Configure(EntityTypeBuilder<OrderTraveller> builder)
        {
            builder.ToTable("OrderTravellers", PersistenceSchemas.Order, table =>
            {
                table.HasCheckConstraint("CK_OrderTravellers_Guardian", "[InfantParentTravellerId] IS NULL OR [InfantParentTravellerId] <> [Id]");
                table.RequiredEnum<PassengerTypeCode>("OrderTravellers", "PassengerTypeCode");
            });
            builder.HasKey(traveller => traveller.Id);
            builder.Property(traveller => traveller.Id).ValueGeneratedNever();
            builder.Property(traveller => traveller.SourceTravellerRef).HasMaxLength(PersistenceSchemas.ReferenceLength).IsRequired();
            builder.Property(traveller => traveller.ClientTravellerRef).HasMaxLength(PersistenceSchemas.ReferenceLength).IsRequired();

            builder.OwnsOne(traveller => traveller.Identity, identity =>
            {
                identity.ToTable("OrderTravellerIdentities", PersistenceSchemas.Order);
                identity.WithOwner().HasForeignKey("TravellerId");
                identity.Property(value => value.GivenName).HasMaxLength(PersistenceSchemas.PersonNameLength).IsRequired();
                identity.Property(value => value.Surname).HasMaxLength(PersistenceSchemas.PersonNameLength).IsRequired();
                identity.Property(value => value.DateOfBirth).IsRequired();
            });

            builder.HasOne<OrderTraveller>().WithMany()
                .HasForeignKey(traveller => new { traveller.OrderId, traveller.InfantParentTravellerId })
                .HasPrincipalKey(parent => new { parent.OrderId, parent.Id })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasAlternateKey(traveller => new { traveller.OrderId, traveller.Id });
            builder.HasIndex(traveller => new { traveller.OrderId, traveller.SourceTravellerRef }).IsUnique();
            builder.HasIndex(traveller => new { traveller.OrderId, traveller.ClientTravellerRef }).IsUnique();
        }
    }
}
