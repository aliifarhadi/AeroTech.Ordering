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
            money.Property(value => value.CurrencyId).HasColumnName($"{prefix}CurrencyId").IsRequired();
        }

        public static void MapBaggage<TOwner>(this OwnedNavigationBuilder<TOwner, BaggageAllowance> baggage, string prefix)
            where TOwner : class
        {
            baggage.Property(value => value.Pieces).HasColumnName($"{prefix}Pieces");
            baggage.Property(value => value.Weight).HasColumnName($"{prefix}Weight").HasQuantityPrecision();
            baggage.Property(value => value.WeightUnit).HasColumnName($"{prefix}WeightUnit");
        }

        public static void MapAppliedConversion<TOwner>(this OwnedNavigationBuilder<TOwner, AppliedConversion> conversion, string prefix)
            where TOwner : class
        {
            conversion.Property(value => value.SourceConversionRef).HasColumnName($"{prefix}SourceRef").HasMaxLength(PersistenceSchemas.ReferenceLength).IsRequired();
            conversion.Property(value => value.FromCurrencyId).HasColumnName($"{prefix}FromCurrencyId").IsRequired();
            conversion.Property(value => value.ToCurrencyId).HasColumnName($"{prefix}ToCurrencyId").IsRequired();
            conversion.Property(value => value.Rate).HasColumnName($"{prefix}Rate").HasRatePrecision().IsRequired();
            conversion.Property(value => value.DecimalPlaces).HasColumnName($"{prefix}DecimalPlaces").IsRequired();
            conversion.Property(value => value.RoundingToken).HasColumnName($"{prefix}RoundingToken").HasMaxLength(PersistenceSchemas.ReferenceLength);
        }
    }
}
