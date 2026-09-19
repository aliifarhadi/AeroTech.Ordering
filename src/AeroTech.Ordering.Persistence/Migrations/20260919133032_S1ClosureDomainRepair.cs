using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ordering.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class S1ClosureDomainRepair : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            Guard(migrationBuilder);
            RenameInitiatingActor(migrationBuilder);
            AddSalesProvenanceColumns(migrationBuilder);
            BackfillSalesProvenance(migrationBuilder);
            AddSettlementAttribution(migrationBuilder);
            AddComponentTotals(migrationBuilder);
            BackfillComponentTotals(migrationBuilder);
            AddFulfillmentProfileMembers(migrationBuilder);
            AddFundingObligationScope(migrationBuilder);
        }

        private static void Guard(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM [Order].[OrderServices] WHERE [CommercialStatus] NOT IN (1, 2, 3))
    THROW 52010, 'An order service holds a commercial status outside Pending/Active/Cancelled; the lifecycle vocabulary correction cannot reinterpret it without an owner decision.', 1;
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM [Order].[OrderItems] WHERE [CommercialStatus] NOT IN (1, 2, 3))
    THROW 52011, 'An order item holds a commercial status outside Active/Replaced/Cancelled; the lifecycle vocabulary correction cannot reinterpret it without an owner decision.', 1;
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM [Order].[PricingLines] WHERE [Effect] = 2)
    THROW 52012, 'A settlement-only pricing line exists without settlement attribution; attribution is a source fact and is never invented by a migration.', 1;
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM [Order].[FundingObligations] WHERE [OrderItemId] IS NULL)
    THROW 52013, 'A funding obligation has no item scope; its scope is not derivable and is never invented by a migration.', 1;
");

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM [Order].[PricingLines] line
    INNER JOIN [Order].[Orders] ord ON ord.[Id] = line.[OrderId]
    WHERE line.[SaleValueCurrencyRef] <> ord.[SaleCurrencyRef])
    THROW 52014, 'A pricing line is valued in a currency other than the order sale currency; component totals cannot be derived deterministically.', 1;
");
        }

        private static void RenameInitiatingActor(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BuyerActorContextType",
                schema: "Order",
                table: "Orders",
                newName: "InitiatingActorContextType");

            migrationBuilder.RenameColumn(
                name: "BuyerActorId",
                schema: "Order",
                table: "Orders",
                newName: "InitiatingActorId");
        }

        private static void AddSalesProvenanceColumns(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SellerContextType",
                schema: "Order",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SellerId",
                schema: "Order",
                table: "Orders",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SellingOfficeKind",
                schema: "Order",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BuyerContextType",
                schema: "Order",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "BuyerId",
                schema: "Order",
                table: "Orders",
                type: "bigint",
                nullable: true);
        }

        private static void BackfillSalesProvenance(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE [Order].[Orders]
SET [SellingOfficeKind] = CASE [Channel]
        WHEN 1 THEN 1
        WHEN 4 THEN 2
        WHEN 3 THEN 2
        ELSE 0
    END
WHERE [SellingOfficeId] IS NOT NULL;
");

            migrationBuilder.Sql(@"
UPDATE [Order].[Orders]
SET [SellerContextType] = 1,
    [SellerId] = [OwnerAirlineId]
WHERE [Channel] = 1;
");
        }

        private static void AddSettlementAttribution(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SettlementPartyRef",
                schema: "Order",
                table: "PricingLines",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SettlementCategoryCode",
                schema: "Order",
                table: "PricingLines",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_PricingLines_SettlementAttribution",
                schema: "Order",
                table: "PricingLines",
                sql: "([Effect] = 2 AND [SettlementPartyRef] IS NOT NULL AND [SettlementCategoryCode] IS NOT NULL) OR ([Effect] <> 2 AND [SettlementPartyRef] IS NULL AND [SettlementCategoryCode] IS NULL)");
        }

        private static void AddComponentTotals(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderComponentTotals",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    Component = table.Column<int>(type: "int", nullable: false),
                    Effect = table.Column<int>(type: "int", nullable: false),
                    DebitAmount = table.Column<decimal>(type: "decimal(28,8)", precision: 28, scale: 8, nullable: false),
                    CreditAmount = table.Column<decimal>(type: "decimal(28,8)", precision: 28, scale: 8, nullable: false),
                    CurrencyRef = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderComponentTotals", x => x.Id);
                    table.CheckConstraint("CK_OrderComponentTotals_Magnitudes", "[DebitAmount] >= 0 AND [CreditAmount] >= 0");
                    table.ForeignKey(
                        name: "FK_OrderComponentTotals_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "Order",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderComponentTotals_OrderId_Component_Effect",
                schema: "Order",
                table: "OrderComponentTotals",
                columns: new[] { "OrderId", "Component", "Effect" },
                unique: true);
        }

        private static void BackfillComponentTotals(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
WITH totals AS (
    SELECT line.[OrderId] AS OrderId,
           line.[Component] AS Component,
           line.[Effect] AS Effect,
           SUM(CASE WHEN line.[Direction] = 1 THEN line.[SaleValueAmount] ELSE 0 END) AS DebitAmount,
           SUM(CASE WHEN line.[Direction] = 2 THEN line.[SaleValueAmount] ELSE 0 END) AS CreditAmount,
           MIN(line.[SaleValueCurrencyRef]) AS CurrencyRef
    FROM [Order].[PricingLines] line
    GROUP BY line.[OrderId], line.[Component], line.[Effect])
INSERT INTO [Order].[OrderComponentTotals]
    ([Id], [OrderId], [Component], [Effect], [DebitAmount], [CreditAmount], [CurrencyRef], [LastUpdateTime])
SELECT ROW_NUMBER() OVER (ORDER BY totals.OrderId, totals.Component, totals.Effect),
       totals.OrderId,
       totals.Component,
       totals.Effect,
       totals.DebitAmount,
       totals.CreditAmount,
       totals.CurrencyRef,
       SYSDATETIMEOFFSET()
FROM totals;
");
        }

        private static void AddFulfillmentProfileMembers(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocumentAuthority",
                schema: "Order",
                table: "OrderServices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResourceUnitPolicyRef",
                schema: "Order",
                table: "OrderServices",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryControlPolicyRef",
                schema: "Order",
                table: "OrderServices",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DependencyTreatmentPolicyRef",
                schema: "Order",
                table: "OrderServices",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PartialFulfillmentSupported",
                schema: "Order",
                table: "OrderServices",
                type: "bit",
                nullable: true);
        }

        private static void AddFundingObligationScope(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OrderServiceId",
                schema: "Order",
                table: "FundingObligations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PricingLineId",
                schema: "Order",
                table: "FundingObligations",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FundingObligations_OrderServiceId",
                schema: "Order",
                table: "FundingObligations",
                column: "OrderServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingObligations_PricingLineId",
                schema: "Order",
                table: "FundingObligations",
                column: "PricingLineId");

            migrationBuilder.AddForeignKey(
                name: "FK_FundingObligations_OrderServices_OrderServiceId",
                schema: "Order",
                table: "FundingObligations",
                column: "OrderServiceId",
                principalSchema: "Order",
                principalTable: "OrderServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FundingObligations_PricingLines_PricingLineId",
                schema: "Order",
                table: "FundingObligations",
                column: "PricingLineId",
                principalSchema: "Order",
                principalTable: "PricingLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddCheckConstraint(
                name: "CK_FundingObligations_ExactlyOneScope",
                schema: "Order",
                table: "FundingObligations",
                sql: "(CASE WHEN [OrderItemId] IS NULL THEN 0 ELSE 1 END) + (CASE WHEN [OrderServiceId] IS NULL THEN 0 ELSE 1 END) + (CASE WHEN [PricingLineId] IS NULL THEN 0 ELSE 1 END) = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FundingObligations_ExactlyOneScope",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropForeignKey(
                name: "FK_FundingObligations_OrderServices_OrderServiceId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropForeignKey(
                name: "FK_FundingObligations_PricingLines_PricingLineId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropIndex(
                name: "IX_FundingObligations_OrderServiceId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropIndex(
                name: "IX_FundingObligations_PricingLineId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropColumn(
                name: "OrderServiceId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropColumn(
                name: "PricingLineId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropColumn(
                name: "DocumentAuthority",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "ResourceUnitPolicyRef",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "DeliveryControlPolicyRef",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "DependencyTreatmentPolicyRef",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "PartialFulfillmentSupported",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropTable(
                name: "OrderComponentTotals",
                schema: "Order");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PricingLines_SettlementAttribution",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "SettlementPartyRef",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "SettlementCategoryCode",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "SellerContextType",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SellerId",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SellingOfficeKind",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BuyerContextType",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BuyerId",
                schema: "Order",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "InitiatingActorId",
                schema: "Order",
                table: "Orders",
                newName: "BuyerActorId");

            migrationBuilder.RenameColumn(
                name: "InitiatingActorContextType",
                schema: "Order",
                table: "Orders",
                newName: "BuyerActorContextType");
        }
    }
}
