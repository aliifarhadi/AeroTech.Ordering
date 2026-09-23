using AeroTech.Messages.Ordering.Enums;
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
                table.RequiredEnum<OrderServiceType>("OrderServices", "ServiceType");
                table.RequiredEnum<OrderServiceCommercialStatus>("OrderServices", "CommercialStatus");
                table.RequiredEnum<FulfillmentProfileAssurance>("OrderServices", "FulfillmentProfileAssurance");
                table.RequiredEnum<ReservationRequirement>("OrderServices", "ReservationRequirement");
                table.RequiredEnum<FulfillmentDocumentKind>("OrderServices", "FulfillmentDocumentKind");
                table.OptionalEnum<DocumentAuthority>("OrderServices", "DocumentAuthority");
                table.RequiredEnum<FundingRequirement>("OrderServices", "FundingRequirement");
                table.OptionalEnum<BaggageWeightUnit>("OrderServices", "CheckedBaggageWeightUnit");
                table.OptionalEnum<BaggageWeightUnit>("OrderServices", "CabinBaggageWeightUnit");
            });
            builder.HasKey(service => service.Id);
            builder.Property(service => service.Id).ValueGeneratedNever();

            builder.HasDiscriminator(service => service.ServiceType)
                .HasValue<OrderAirTransportService>(OrderServiceType.AirTransportation);

            builder.OwnsOne(service => service.FulfillmentProfile, profile =>
            {
                profile.Property(value => value.ProfileRef).HasColumnName("FulfillmentProfileRef").HasMaxLength(PersistenceSchemas.ProfileLength);
                profile.Property(value => value.ProfileVersion).HasColumnName("FulfillmentProfileVersion").HasMaxLength(PersistenceSchemas.ProfileLength);
                profile.Property(value => value.Assurance).HasColumnName("FulfillmentProfileAssurance").IsRequired();
                profile.Property(value => value.ReservationRequirement).HasColumnName("ReservationRequirement").IsRequired();
                profile.Property(value => value.DocumentKind).HasColumnName("FulfillmentDocumentKind").IsRequired();
                profile.Property(value => value.DocumentAuthority).HasColumnName("DocumentAuthority");
                profile.Property(value => value.FundingRequirement).HasColumnName("FundingRequirement").IsRequired();
                profile.Property(value => value.CapacityUnits).HasColumnName("CapacityUnits");
                profile.Property(value => value.ResourceUnitPolicyRef).HasColumnName("ResourceUnitPolicyRef").HasMaxLength(PersistenceSchemas.ReferenceLength);
                profile.Property(value => value.DeliveryControlPolicyRef).HasColumnName("DeliveryControlPolicyRef").HasMaxLength(PersistenceSchemas.ReferenceLength);
                profile.Property(value => value.DependencyTreatmentPolicyRef).HasColumnName("DependencyTreatmentPolicyRef").HasMaxLength(PersistenceSchemas.ReferenceLength);
                profile.Property(value => value.PartialFulfillmentSupported).HasColumnName("PartialFulfillmentSupported");
                profile.Ignore(value => value.IsCertified);
            });
            builder.Navigation(service => service.FulfillmentProfile).IsRequired();

            builder.HasAlternateKey(service => new { service.OrderId, service.Id });

            builder.HasOne<OrderItem>().WithMany()
                .HasForeignKey(service => new { service.OrderId, service.OrderItemId })
                .HasPrincipalKey(item => new { item.OrderId, item.Id })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<OrderChange>().WithMany()
                .HasForeignKey(service => service.CreatedByChangeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(service => new { service.OrderItemId, service.CommercialStatus });
        }
    }

    public sealed class OrderAirTransportServiceConfiguration : IEntityTypeConfiguration<OrderAirTransportService>
    {
        public void Configure(EntityTypeBuilder<OrderAirTransportService> builder)
        {
            builder.Property(service => service.BookingClass).HasMaxLength(PersistenceSchemas.ReferenceLength);

            builder.OwnsOne(service => service.SoldTerms, terms =>
            {
                terms.Property(value => value.Refundable).HasColumnName("SoldTermRefundable");
                terms.Property(value => value.Changeable).HasColumnName("SoldTermChangeable");
                terms.Property(value => value.Upgradable).HasColumnName("SoldTermUpgradable");
                terms.Ignore(value => value.SuppliesNothing);
            });

            builder.OwnsOne(service => service.CheckedBaggage, baggage => baggage.MapBaggage("CheckedBaggage"));
            builder.OwnsOne(service => service.CabinBaggage, baggage => baggage.MapBaggage("CabinBaggage"));

            builder.HasOne<OrderTraveller>().WithMany()
                .HasForeignKey(service => new { service.OrderId, service.TravellerId })
                .HasPrincipalKey(traveller => new { traveller.OrderId, traveller.Id })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<OrderSegment>().WithMany()
                .HasForeignKey(service => new { service.OrderId, service.SegmentId })
                .HasPrincipalKey(segment => new { segment.OrderId, segment.Id })
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(service => new { service.OrderId, service.TravellerId, service.SegmentId }).IsUnique();
        }
    }
}
