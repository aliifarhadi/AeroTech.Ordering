using AeroTech.Ordering.Domain.OrderAggregate;
using AeroTech.Ordering.Domain.OrderAggregate.Entities;
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
                table.HasCheckConstraint("CK_Orders_Root", "[RootOrderId] > 0");
            });

            builder.HasKey(order => order.Id);
            builder.Property(order => order.Id).ValueGeneratedNever();
            builder.Ignore(order => order.Channel);

            builder.Property(order => order.OrderReference).HasMaxLength(PersistenceSchemas.OwnerNameLength).IsRequired();
            builder.Property(order => order.SourceOfferId).HasMaxLength(2048).IsRequired();
            builder.Property(order => order.AcceptedSnapshotDigest).HasMaxLength(PersistenceSchemas.DigestLength).IsFixedLength().IsUnicode(false).IsRequired();
            builder.Property(order => order.ClientReference).HasMaxLength(PersistenceSchemas.ReferenceLength);
            builder.Property(order => order.SaleCurrencyCode).HasMaxLength(PersistenceSchemas.CurrencyCodeLength);

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

            builder.OwnsOne(order => order.InitiatingActor, actor =>
            {
                actor.Property(value => value.ContextType).HasColumnName("InitiatingActorContextType").IsRequired();
                actor.Property(value => value.ActorId).HasColumnName("InitiatingActorId");
            });
            builder.Navigation(order => order.InitiatingActor).IsRequired();

            builder.OwnsOne(order => order.Buyer, buyer =>
            {
                buyer.Property(value => value.ContextType).HasColumnName("BuyerContextType");
                buyer.Property(value => value.BuyerId).HasColumnName("BuyerId");
                buyer.Ignore(value => value.IsSupplied);
            });
            builder.Navigation(order => order.Buyer).IsRequired();

            builder.OwnsOne(order => order.CustomerTotal, money => money.MapMoney("CustomerTotal"));

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
            builder.HasMany(order => order.Travellers).WithOne().HasForeignKey(child => child.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.Contacts).WithOne().HasForeignKey(child => child.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.Journeys).WithOne().HasForeignKey(child => child.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.Segments).WithOne().HasForeignKey(child => child.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.Items).WithOne().HasForeignKey(child => child.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.Services).WithOne().HasForeignKey(child => child.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.Changes).WithOne().HasForeignKey(child => child.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.PriceChangeSets).WithOne().HasForeignKey(child => child.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.PricingLines).WithOne().HasForeignKey(child => child.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.FareConstructions).WithOne().HasForeignKey(child => child.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.ItemServiceLinks).WithOne().HasForeignKey(child => child.OrderIdAtAssociation).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.ComponentTotals).WithOne().HasForeignKey(child => child.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(order => order.FundingObligations).WithOne().HasForeignKey(child => child.OrderId).OnDelete(DeleteBehavior.Restrict);

            foreach (var navigation in new[]
                     {
                         nameof(Order.Travellers), nameof(Order.Contacts), nameof(Order.Journeys), nameof(Order.Segments),
                         nameof(Order.Items), nameof(Order.Services), nameof(Order.Changes), nameof(Order.PriceChangeSets),
                         nameof(Order.PricingLines), nameof(Order.FareConstructions), nameof(Order.ItemServiceLinks),
                         nameof(Order.ComponentTotals), nameof(Order.FundingObligations)
                     })
                builder.Navigation(navigation).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
