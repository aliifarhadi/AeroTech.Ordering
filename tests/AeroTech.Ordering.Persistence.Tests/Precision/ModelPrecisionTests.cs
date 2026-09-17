using AeroTech.Ordering.Persistence.Outbox;
using AeroTech.Ordering.Persistence.Tests._Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace AeroTech.Ordering.Persistence.Tests.Precision
{
    [Collection(OrderingDatabaseCollection.Name)]
    public sealed class ModelPrecisionTests
    {
        private const string ReferenceDataNamespace = "AeroTech.Ordering.ReferenceData";

        private readonly OrderingDatabaseFixture _fixture;

        public ModelPrecisionTests(OrderingDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Command_model_has_explicit_precision_on_every_decimal()
        {
            using var context = _fixture.NewCommandContext();

            AssertExplicitDecimalPrecision(context.Model);
        }

        [Fact]
        public void Query_model_has_explicit_precision_on_every_owned_decimal()
        {
            using var context = _fixture.NewQueryContext();

            AssertExplicitDecimalPrecision(context.Model);
        }

        [Fact]
        public void Command_model_has_no_blanket_string_length()
        {
            using var context = _fixture.NewCommandContext();

            var payload = context.Model.FindEntityType(typeof(OutboxMessage))!.FindProperty(nameof(OutboxMessage.Payload))!;

            Assert.Null(payload.GetMaxLength());
            Assert.Equal("nvarchar(max)", payload.GetColumnType());
        }

        private static void AssertExplicitDecimalPrecision(IModel model)
        {
            var violations = model.GetEntityTypes()
                .Where(entity => !(entity.ClrType.Namespace ?? string.Empty).StartsWith(ReferenceDataNamespace, StringComparison.Ordinal))
                .SelectMany(entity => entity.GetProperties())
                .Where(property => (Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType) == typeof(decimal))
                .Where(property => property.GetPrecision() is null || property.GetScale() is null)
                .Select(property => $"{property.DeclaringType.DisplayName()}.{property.Name}")
                .ToList();

            Assert.True(violations.Count == 0, $"Decimal properties without explicit precision: {string.Join(", ", violations)}");
        }
    }
}
