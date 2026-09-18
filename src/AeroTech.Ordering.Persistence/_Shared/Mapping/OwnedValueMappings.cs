using AeroTech.Ordering.Domain._Shared.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ordering.Persistence._Shared.Mapping
{
    public static class OwnedValueMappings
    {
        public static void MapMoney<TOwner>(this OwnedNavigationBuilder<TOwner, Money> money, string prefix)
            where TOwner : class
        {
            money.Property(value => value.Amount).HasColumnName($"{prefix}Amount").HasAmountPrecision().IsRequired();
            money.Property(value => value.CurrencyRef).HasColumnName($"{prefix}CurrencyRef").HasMaxLength(PersistenceSchemas.CurrencyRefLength).IsRequired();
        }

        public static void MapValidity<TOwner>(this OwnedNavigationBuilder<TOwner, ValidityFact> validity, string prefix)
            where TOwner : class
        {
            validity.Property(value => value.State).HasColumnName($"{prefix}State").IsRequired();
            validity.Property(value => value.Value).HasColumnName($"{prefix}Value");
            validity.Property(value => value.Owner).HasColumnName($"{prefix}Owner").HasMaxLength(PersistenceSchemas.OwnerNameLength).IsRequired();
            validity.Property(value => value.SourceRef).HasColumnName($"{prefix}SourceRef").HasMaxLength(PersistenceSchemas.ReferenceLength);
            validity.Property(value => value.Reason).HasColumnName($"{prefix}Reason").HasMaxLength(PersistenceSchemas.ReasonLength);
        }
    }
}
