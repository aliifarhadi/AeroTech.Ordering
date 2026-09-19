using AeroTech.Ordering.Domain.OrderAggregate;
using AeroTech.Ordering.Domain.OrderPreparationAggregate;
using AeroTech.Ordering.Persistence._Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence.OrderAggregate
{
    public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public const string ReferenceIndex = "UX_Orders_Owner_Reference";
        public const string SourcePreparationIndex = "UX_Orders_Owner_SourcePreparation";

        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders", PersistenceSchemas.Order, table =>
            {
                table.HasCheckConstraint("CK_Orders_Revisions", "[CommercialVersion] >= 1 AND [FinancialSequence] >= 0 AND [OrderRevision] >= 1 AND [LastEventOrdinal] >= 0");
                table.HasCheckConstraint("CK_Orders_Root", "[RootOrderId] = [Id]");
            });

            builder.HasKey(order => order.Id);
            builder.Property(order => order.Id).ValueGeneratedNever();
            builder.Ignore(order => order.IsSandboxScoped);
            builder.Ignore(order => order.Channel);
            builder.Ignore(order => order.SellingOfficeKind);
            builder.Ignore(order => order.SellingOfficeId);

            builder.OwnsOne(order => order.SalesContext, sales =>
            {
                sales.Property(value => value.Channel).HasColumnName("Channel").IsRequired();
                sales.Property(value => value.SellerContextType).HasColumnName("SellerContextType");
                sales.Property(value => value.SellerId).HasColumnName("SellerId");
                sales.Property(value => value.SellingOfficeKind).HasColumnName("SellingOfficeKind");
                sales.Property(value => value.SellingOfficeId).HasColumnName("SellingOfficeId");
                sales.Ignore(value => value.HasSeller);
                sales.Ignore(value => value.HasSellingOffice);
            });
            builder.Navigation(order => order.SalesContext).IsRequired();

            builder.OwnsOne(order => order.Buyer, buyer =>
            {
                buyer.Property(value => value.ContextType).HasColumnName("BuyerContextType");
                buyer.Property(value => value.BuyerId).HasColumnName("BuyerId");
                buyer.Ignore(value => value.IsSupplied);
            });
            builder.Navigation(order => order.Buyer).IsRequired();

            builder.OwnsOne(order => order.InitiatingActor, actor =>
            {
                actor.Property(value => value.ContextType).HasColumnName("InitiatingActorContextType").IsRequired();
                actor.Property(value => value.ActorId).HasColumnName("InitiatingActorId");
            });
            builder.Navigation(order => order.InitiatingActor).IsRequired();

            builder.Property(order => order.OrderReference).HasMaxLength(PersistenceSchemas.OwnerNameLength).IsRequired();
            builder.OwnsOne(order => order.SaleCurrency, currency => currency.MapCurrency("SaleCurrency"));
            builder.Property(order => order.ClientReference).HasMaxLength(PersistenceSchemas.ReferenceLength);

            builder.OwnsOne(order => order.CustomerTotal, money => money.MapMoney("CustomerTotal"));
            builder.OwnsOne(order => order.OfferValidity, validity => validity.MapValidity("OfferValidity"));
            builder.OwnsOne(order => order.PriceValidity, validity => validity.MapValidity("PriceValidity"));
            builder.OwnsOne(order => order.TicketingValidity, validity => validity.MapValidity("TicketingValidity"));
            builder.OwnsOne(order => order.ObservedTicketingDeadline, observed => observed.MapObservedTime("ObservedTicketingDeadline"));
            builder.Property(order => order.SourceJourneyTypeRaw).HasMaxLength(PersistenceSchemas.ReferenceLength);

            builder.OwnsOne(order => order.AcceptedSource, source =>
            {
                source.Property(value => value.PreparationId).HasColumnName("AcceptedPreparationId");
                source.Property(value => value.SourceOwner).HasColumnName("AcceptedSourceOwner").HasMaxLength(PersistenceSchemas.OwnerNameLength).IsRequired();
                source.Property(value => value.SourceOfferId).HasColumnName("AcceptedSourceOfferId").HasMaxLength(2048).IsRequired();
                source.Property(value => value.ProviderProfileId).HasColumnName("AcceptedProviderProfileId").HasMaxLength(PersistenceSchemas.ProfileLength).IsRequired();
                source.Property(value => value.ContractVersion).HasColumnName("AcceptedContractVersion").HasMaxLength(PersistenceSchemas.ProfileLength).IsRequired();
                source.Property(value => value.AcceptanceProfile).HasColumnName("AcceptanceProfile").HasMaxLength(PersistenceSchemas.ProfileLength).IsRequired();
                source.Property(value => value.AcceptanceAssurance).HasColumnName("AcceptanceAssurance");
                source.Property(value => value.OwnerBindingRef).HasColumnName("AcceptedOwnerBindingRef").HasMaxLength(PersistenceSchemas.ReferenceLength);
                source.Property(value => value.SnapshotDigest).HasColumnName("AcceptedSnapshotDigest").HasMaxLength(PersistenceSchemas.DigestLength).IsFixedLength().IsUnicode(false).IsRequired();
                source.Property(value => value.SourcePayloadHash).HasColumnName("AcceptedSourcePayloadHash").HasMaxLength(PersistenceSchemas.DigestLength).IsRequired();
                source.Property(value => value.PricedAt).HasColumnName("SourcePricedAt");
                source.Property(value => value.CapturedAt).HasColumnName("SourceCapturedAt");
                source.Property(value => value.ClientAcceptedAt).HasColumnName("ClientAcceptedAt");
                source.Property(value => value.AcceptedAt).HasColumnName("AcceptedAt");
                source.Ignore(value => value.IsSandboxScoped);
            });

            builder.HasOne<OrderPreparation>()
                .WithMany()
                .HasForeignKey(order => order.SourcePreparationId)
                .OnDelete(DeleteBehavior.Restrict);

            MapChildren(builder);

            builder.HasIndex(order => new { order.OwnerAirlineId, order.OrderReference }).IsUnique().HasDatabaseName(ReferenceIndex);
            builder.HasIndex(order => new { order.OwnerAirlineId, order.SourcePreparationId }).IsUnique().HasDatabaseName(SourcePreparationIndex);
            builder.HasIndex(order => new { order.OwnerAirlineId, order.FinancialCustomerId, order.CreatedAt });
            builder.HasIndex(order => new { order.OwnerAirlineId, order.CommercialSummary });
        }

        private static void MapChildren(EntityTypeBuilder<Order> builder)
        {
            builder.HasMany(order => order.Travelers).WithOne().HasForeignKey(child => child.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.Contacts).WithOne().HasForeignKey(child => child.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.Journeys).WithOne().HasForeignKey(child => child.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.Segments).WithOne().HasForeignKey(child => child.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.Items).WithOne().HasForeignKey(child => child.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.Services).WithOne().HasForeignKey(child => child.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.ItemServiceLinks).WithOne().HasForeignKey(child => child.OrderIdAtAssociation).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.Changes).WithOne().HasForeignKey(child => child.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.PriceChangeSets).WithOne().HasForeignKey(child => child.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.PricingLines).WithOne().HasForeignKey(child => child.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.FareConstructions).WithOne().HasForeignKey(child => child.OrderIdAtCreation).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.FundingObligations).WithOne().HasForeignKey(child => child.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.ComponentTotals).WithOne().HasForeignKey(child => child.OrderId).OnDelete(DeleteBehavior.Restrict);

            foreach (var navigation in new[]
                     {
                         nameof(Order.Travelers), nameof(Order.Contacts), nameof(Order.Journeys), nameof(Order.Segments), nameof(Order.Items), nameof(Order.Services),
                         nameof(Order.ItemServiceLinks), nameof(Order.Changes), nameof(Order.PriceChangeSets), nameof(Order.PricingLines),
                         nameof(Order.FareConstructions), nameof(Order.FundingObligations), nameof(Order.ComponentTotals)
                     })
                builder.Navigation(navigation).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
