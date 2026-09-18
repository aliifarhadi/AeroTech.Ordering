using AeroTech.Ordering.Domain.OrderAggregate.Entities;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class OrderServiceConfiguration : IEntityTypeConfiguration<OrderService>
    {
        public void Configure(EntityTypeBuilder<OrderService> builder)
        {
            builder.ToTable("OrderServices", PersistenceSchemas.Order, table =>
            {
                table.HasCheckConstraint("CK_OrderServices_Quantity", "[Quantity] > 0");
                table.HasCheckConstraint("CK_OrderServices_Version", "[ServiceVersion] >= 1");
                table.HasCheckConstraint("CK_OrderServices_CapacityUnits", "[CapacityUnits] IS NULL OR [CapacityUnits] >= 0");
            });
            builder.HasKey(service => service.Id);
            builder.Property(service => service.Id).ValueGeneratedNever();
            builder.Property(service => service.SourceServiceRef).HasMaxLength(PersistenceSchemas.ReferenceLength).IsRequired();
            builder.Property(service => service.ServiceCode).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(service => service.Name).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(service => service.SupplierPartyRef).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(service => service.DeliveryProviderRef).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(service => service.Quantity).HasQuantityPrecision();
            builder.Property(service => service.DetailSchema).HasMaxLength(PersistenceSchemas.OwnerNameLength).IsRequired();
            builder.OwnsOne(service => service.SoldTerms, terms =>
            {
                terms.Property(value => value.Refundable).HasColumnName("SoldTermRefundable");
                terms.Property(value => value.Changeable).HasColumnName("SoldTermChangeable");
                terms.Property(value => value.Upgradable).HasColumnName("SoldTermUpgradable");
                terms.Ignore(value => value.SuppliesNothing);
            });
            builder.OwnsOne(service => service.FulfillmentProfile, profile =>
            {
                profile.Property(value => value.ProfileRef).HasColumnName("FulfillmentProfileRef").HasMaxLength(PersistenceSchemas.ProfileLength).IsRequired();
                profile.Property(value => value.ProfileVersion).HasColumnName("FulfillmentProfileVersion").HasMaxLength(PersistenceSchemas.ProfileLength).IsRequired();
                profile.Property(value => value.Assurance).HasColumnName("FulfillmentProfileAssurance");
                profile.Property(value => value.ReservationRequirement).HasColumnName("ReservationRequirement");
                profile.Property(value => value.DocumentKind).HasColumnName("DocumentKind");
                profile.Property(value => value.FundingRequirement).HasColumnName("FundingRequirement");
                profile.Property(value => value.CapacityUnits).HasColumnName("CapacityUnits");
                profile.Ignore(value => value.IsCertified);
            });
            builder.OwnsOne(service => service.AirTransport, detail =>
            {
                detail.ToTable("AirTransportServiceDetails", PersistenceSchemas.Order);
                detail.WithOwner().HasForeignKey("ServiceId");
                detail.Property(value => value.CabinRef).HasMaxLength(PersistenceSchemas.ReferenceLength);
                detail.Property(value => value.RbdRef).HasMaxLength(PersistenceSchemas.ReferenceLength);
                detail.Property(value => value.BookingClass).HasMaxLength(PersistenceSchemas.ReferenceLength);
                detail.OwnsOne(value => value.CheckedBaggage, baggage => baggage.MapBaggage("CheckedBaggage"));
                detail.OwnsOne(value => value.CabinBaggage, baggage => baggage.MapBaggage("CabinBaggage"));
            });
            builder.HasOne<OrderItem>().WithMany().HasForeignKey(service => service.OrderItemId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<OrderChange>().WithMany().HasForeignKey(service => service.CreatedByChangeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(service => service.Beneficiaries).WithOne().HasForeignKey(beneficiary => beneficiary.ServiceId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(service => service.Coverage).WithOne().HasForeignKey(coverage => coverage.ServiceId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(service => service.Beneficiaries).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(service => service.Coverage).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Ignore(service => service.SoleCoveredSegmentId);
            builder.HasIndex(service => new { service.OrderId, service.SourceServiceRef }).IsUnique();
            builder.HasIndex(service => new { service.OrderItemId, service.CommercialStatus });
        }
    }
}
