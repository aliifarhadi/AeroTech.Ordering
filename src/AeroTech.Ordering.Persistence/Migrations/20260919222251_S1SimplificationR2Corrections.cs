using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ordering.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class S1SimplificationR2Corrections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderServices_OrderId_TravellerId_SegmentId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_Root",
                schema: "Order",
                table: "Orders");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                schema: "Order",
                table: "PricingLines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<long>(
                name: "TravellerId",
                schema: "Order",
                table: "OrderServices",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "SegmentId",
                schema: "Order",
                table: "OrderServices",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<int>(
                name: "CapacityUnits",
                schema: "Order",
                table: "OrderServices",
                type: "int",
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

            migrationBuilder.AddColumn<int>(
                name: "DocumentAuthority",
                schema: "Order",
                table: "OrderServices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FulfillmentDocumentKind",
                schema: "Order",
                table: "OrderServices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FulfillmentProfileAssurance",
                schema: "Order",
                table: "OrderServices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FulfillmentProfileRef",
                schema: "Order",
                table: "OrderServices",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FulfillmentProfileVersion",
                schema: "Order",
                table: "OrderServices",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FundingRequirement",
                schema: "Order",
                table: "OrderServices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "PartialFulfillmentSupported",
                schema: "Order",
                table: "OrderServices",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReservationRequirement",
                schema: "Order",
                table: "OrderServices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ResourceUnitPolicyRef",
                schema: "Order",
                table: "OrderServices",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServiceType",
                schema: "Order",
                table: "OrderServices",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.AddColumn<string>(
                name: "SaleCurrencyCode",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "OrderItemId",
                schema: "Order",
                table: "FundingObligations",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "OrderServiceId",
                schema: "Order",
                table: "FundingObligations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PriceChangeSetId",
                schema: "Order",
                table: "FundingObligations",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "PricingLineId",
                schema: "Order",
                table: "FundingObligations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceConstructionType",
                schema: "Order",
                table: "FarePricingUnits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Assurance",
                schema: "Order",
                table: "FareConstructions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OrderComponentTotals",
                schema: "Order",
                columns: table => new
                {
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    Component = table.Column<int>(type: "int", nullable: false),
                    Effect = table.Column<int>(type: "int", nullable: false),
                    DebitAmount = table.Column<decimal>(type: "decimal(28,8)", precision: 28, scale: 8, nullable: false),
                    CreditAmount = table.Column<decimal>(type: "decimal(28,8)", precision: 28, scale: 8, nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderComponentTotals", x => new { x.OrderId, x.Component, x.Effect });
                    table.CheckConstraint("CK_OrderComponentTotals_Magnitudes", "[DebitAmount] >= 0 AND [CreditAmount] >= 0");
                    table.ForeignKey(
                        name: "FK_OrderComponentTotals_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "Order",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItemServiceLinks",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OrderIdAtAssociation = table.Column<long>(type: "bigint", nullable: false),
                    OrderItemId = table.Column<long>(type: "bigint", nullable: false),
                    OrderServiceId = table.Column<long>(type: "bigint", nullable: false),
                    LinkedByChangeId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemServiceLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItemServiceLinks_OrderChanges_LinkedByChangeId",
                        column: x => x.LinkedByChangeId,
                        principalSchema: "Order",
                        principalTable: "OrderChanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemServiceLinks_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalSchema: "Order",
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemServiceLinks_OrderServices_OrderServiceId",
                        column: x => x.OrderServiceId,
                        principalSchema: "Order",
                        principalTable: "OrderServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemServiceLinks_Orders_OrderIdAtAssociation",
                        column: x => x.OrderIdAtAssociation,
                        principalSchema: "Order",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderServices_OrderId",
                schema: "Order",
                table: "OrderServices",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderServices_OrderId_TravellerId_SegmentId",
                schema: "Order",
                table: "OrderServices",
                columns: new[] { "OrderId", "TravellerId", "SegmentId" },
                unique: true,
                filter: "[TravellerId] IS NOT NULL AND [SegmentId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_Root",
                schema: "Order",
                table: "Orders",
                sql: "[RootOrderId] > 0");

            migrationBuilder.CreateIndex(
                name: "IX_FundingObligations_OrderServiceId",
                schema: "Order",
                table: "FundingObligations",
                column: "OrderServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingObligations_PriceChangeSetId",
                schema: "Order",
                table: "FundingObligations",
                column: "PriceChangeSetId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingObligations_PricingLineId",
                schema: "Order",
                table: "FundingObligations",
                column: "PricingLineId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FundingObligations_ExactlyOneScope",
                schema: "Order",
                table: "FundingObligations",
                sql: "(CASE WHEN [OrderItemId] IS NULL THEN 0 ELSE 1 END) + (CASE WHEN [OrderServiceId] IS NULL THEN 0 ELSE 1 END) + (CASE WHEN [PricingLineId] IS NULL THEN 0 ELSE 1 END) = 1");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemServiceLinks_LinkedByChangeId",
                schema: "Order",
                table: "OrderItemServiceLinks",
                column: "LinkedByChangeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemServiceLinks_OrderIdAtAssociation",
                schema: "Order",
                table: "OrderItemServiceLinks",
                column: "OrderIdAtAssociation");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemServiceLinks_OrderItemId_OrderServiceId",
                schema: "Order",
                table: "OrderItemServiceLinks",
                columns: new[] { "OrderItemId", "OrderServiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemServiceLinks_OrderServiceId",
                schema: "Order",
                table: "OrderItemServiceLinks",
                column: "OrderServiceId");

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
                name: "FK_FundingObligations_PriceChangeSets_PriceChangeSetId",
                schema: "Order",
                table: "FundingObligations",
                column: "PriceChangeSetId",
                principalSchema: "Order",
                principalTable: "PriceChangeSets",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FundingObligations_OrderServices_OrderServiceId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropForeignKey(
                name: "FK_FundingObligations_PriceChangeSets_PriceChangeSetId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropForeignKey(
                name: "FK_FundingObligations_PricingLines_PricingLineId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropTable(
                name: "OrderComponentTotals",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "OrderItemServiceLinks",
                schema: "Order");

            migrationBuilder.DropIndex(
                name: "IX_OrderServices_OrderId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropIndex(
                name: "IX_OrderServices_OrderId_TravellerId_SegmentId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_Root",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_FundingObligations_OrderServiceId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropIndex(
                name: "IX_FundingObligations_PriceChangeSetId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropIndex(
                name: "IX_FundingObligations_PricingLineId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FundingObligations_ExactlyOneScope",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropColumn(
                name: "Role",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "CapacityUnits",
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
                name: "DocumentAuthority",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "FulfillmentDocumentKind",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "FulfillmentProfileAssurance",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "FulfillmentProfileRef",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "FulfillmentProfileVersion",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "FundingRequirement",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "PartialFulfillmentSupported",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "ReservationRequirement",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "ResourceUnitPolicyRef",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "ServiceType",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "BuyerContextType",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BuyerId",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SaleCurrencyCode",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderServiceId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropColumn(
                name: "PriceChangeSetId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropColumn(
                name: "PricingLineId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropColumn(
                name: "SourceConstructionType",
                schema: "Order",
                table: "FarePricingUnits");

            migrationBuilder.DropColumn(
                name: "Assurance",
                schema: "Order",
                table: "FareConstructions");

            migrationBuilder.AlterColumn<long>(
                name: "TravellerId",
                schema: "Order",
                table: "OrderServices",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "SegmentId",
                schema: "Order",
                table: "OrderServices",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "OrderItemId",
                schema: "Order",
                table: "FundingObligations",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderServices_OrderId_TravellerId_SegmentId",
                schema: "Order",
                table: "OrderServices",
                columns: new[] { "OrderId", "TravellerId", "SegmentId" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_Root",
                schema: "Order",
                table: "Orders",
                sql: "[RootOrderId] = [Id]");
        }
    }
}
