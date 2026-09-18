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

        public static void MapCurrency<TOwner>(this OwnedNavigationBuilder<TOwner, CurrencySnapshot> currency, string prefix)
            where TOwner : class
        {
            currency.Property(value => value.CurrencyRef).HasColumnName($"{prefix}Ref").HasMaxLength(PersistenceSchemas.CurrencyRefLength).IsRequired();
            currency.Property(value => value.CurrencyCode).HasColumnName($"{prefix}Code").HasMaxLength(PersistenceSchemas.CurrencyCodeLength);
        }

        public static void MapObservedTime<TOwner>(this OwnedNavigationBuilder<TOwner, ObservedTimeFact> observed, string prefix)
            where TOwner : class
        {
            observed.Property(value => value.Value).HasColumnName($"{prefix}Value").IsRequired();
            observed.Property(value => value.SourceOwner).HasColumnName($"{prefix}SourceOwner").HasMaxLength(PersistenceSchemas.OwnerNameLength).IsRequired();
            observed.Property(value => value.SourceRef).HasColumnName($"{prefix}SourceRef").HasMaxLength(PersistenceSchemas.ReferenceLength).IsRequired();
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
            conversion.Property(value => value.FromCurrencyRef).HasColumnName($"{prefix}FromCurrencyRef").HasMaxLength(PersistenceSchemas.CurrencyRefLength).IsRequired();
            conversion.Property(value => value.ToCurrencyRef).HasColumnName($"{prefix}ToCurrencyRef").HasMaxLength(PersistenceSchemas.CurrencyRefLength).IsRequired();
            conversion.Property(value => value.Rate).HasColumnName($"{prefix}Rate").HasRatePrecision().IsRequired();
            conversion.Property(value => value.DecimalPlaces).HasColumnName($"{prefix}DecimalPlaces").IsRequired();
            conversion.Property(value => value.RoundingToken).HasColumnName($"{prefix}RoundingToken").HasMaxLength(PersistenceSchemas.ReferenceLength);
        }
    }
}
