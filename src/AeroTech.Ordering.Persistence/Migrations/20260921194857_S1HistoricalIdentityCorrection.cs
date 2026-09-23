using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ordering.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class S1HistoricalIdentityCorrection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FundingObligations_OrderItems_OrderId_OrderItemId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropForeignKey(
                name: "FK_FundingObligations_OrderServices_OrderId_OrderServiceId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropForeignKey(
                name: "FK_FundingObligations_PricingLines_OrderId_PricingLineId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemServiceLinks_OrderItems_OrderIdAtAssociation_OrderItemId",
                schema: "Order",
                table: "OrderItemServiceLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemServiceLinks_OrderServices_OrderIdAtAssociation_OrderServiceId",
                schema: "Order",
                table: "OrderItemServiceLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderServices_OrderChanges_OrderId_CreatedByChangeId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropForeignKey(
                name: "FK_PricingLines_OrderItems_OrderId_OrderItemId",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropIndex(
                name: "IX_OrderServices_OrderId_CreatedByChangeId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropIndex(
                name: "IX_OrderItemServiceLinks_OrderIdAtAssociation_OrderItemId",
                schema: "Order",
                table: "OrderItemServiceLinks");

            migrationBuilder.DropIndex(
                name: "IX_OrderItemServiceLinks_OrderIdAtAssociation_OrderServiceId",
                schema: "Order",
                table: "OrderItemServiceLinks");

            migrationBuilder.DropIndex(
                name: "IX_FundingObligations_OrderId_OrderItemId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropIndex(
                name: "IX_FundingObligations_OrderId_OrderServiceId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropIndex(
                name: "IX_FundingObligations_OrderId_PricingLineId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.CreateIndex(
                name: "IX_PricingLines_OrderItemId",
                schema: "Order",
                table: "PricingLines",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderServices_CreatedByChangeId",
                schema: "Order",
                table: "OrderServices",
                column: "CreatedByChangeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemServiceLinks_OrderServiceId",
                schema: "Order",
                table: "OrderItemServiceLinks",
                column: "OrderServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingObligations_OrderItemId",
                schema: "Order",
                table: "FundingObligations",
                column: "OrderItemId");

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
                name: "FK_FundingObligations_OrderItems_OrderItemId",
                schema: "Order",
                table: "FundingObligations",
                column: "OrderItemId",
                principalSchema: "Order",
                principalTable: "OrderItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemServiceLinks_OrderItems_OrderItemId",
                schema: "Order",
                table: "OrderItemServiceLinks",
                column: "OrderItemId",
                principalSchema: "Order",
                principalTable: "OrderItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemServiceLinks_OrderServices_OrderServiceId",
                schema: "Order",
                table: "OrderItemServiceLinks",
                column: "OrderServiceId",
                principalSchema: "Order",
                principalTable: "OrderServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderServices_OrderChanges_CreatedByChangeId",
                schema: "Order",
                table: "OrderServices",
                column: "CreatedByChangeId",
                principalSchema: "Order",
                principalTable: "OrderChanges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PricingLines_OrderItems_OrderItemId",
                schema: "Order",
                table: "PricingLines",
                column: "OrderItemId",
                principalSchema: "Order",
                principalTable: "OrderItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FundingObligations_OrderItems_OrderItemId",
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

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemServiceLinks_OrderItems_OrderItemId",
                schema: "Order",
                table: "OrderItemServiceLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemServiceLinks_OrderServices_OrderServiceId",
                schema: "Order",
                table: "OrderItemServiceLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderServices_OrderChanges_CreatedByChangeId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropForeignKey(
                name: "FK_PricingLines_OrderItems_OrderItemId",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropIndex(
                name: "IX_PricingLines_OrderItemId",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropIndex(
                name: "IX_OrderServices_CreatedByChangeId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropIndex(
                name: "IX_OrderItemServiceLinks_OrderServiceId",
                schema: "Order",
                table: "OrderItemServiceLinks");

            migrationBuilder.DropIndex(
                name: "IX_FundingObligations_OrderItemId",
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

            migrationBuilder.CreateIndex(
                name: "IX_OrderServices_OrderId_CreatedByChangeId",
                schema: "Order",
                table: "OrderServices",
                columns: new[] { "OrderId", "CreatedByChangeId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemServiceLinks_OrderIdAtAssociation_OrderItemId",
                schema: "Order",
                table: "OrderItemServiceLinks",
                columns: new[] { "OrderIdAtAssociation", "OrderItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemServiceLinks_OrderIdAtAssociation_OrderServiceId",
                schema: "Order",
                table: "OrderItemServiceLinks",
                columns: new[] { "OrderIdAtAssociation", "OrderServiceId" });

            migrationBuilder.CreateIndex(
                name: "IX_FundingObligations_OrderId_OrderItemId",
                schema: "Order",
                table: "FundingObligations",
                columns: new[] { "OrderId", "OrderItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_FundingObligations_OrderId_OrderServiceId",
                schema: "Order",
                table: "FundingObligations",
                columns: new[] { "OrderId", "OrderServiceId" });

            migrationBuilder.CreateIndex(
                name: "IX_FundingObligations_OrderId_PricingLineId",
                schema: "Order",
                table: "FundingObligations",
                columns: new[] { "OrderId", "PricingLineId" });

            migrationBuilder.AddForeignKey(
                name: "FK_FundingObligations_OrderItems_OrderId_OrderItemId",
                schema: "Order",
                table: "FundingObligations",
                columns: new[] { "OrderId", "OrderItemId" },
                principalSchema: "Order",
                principalTable: "OrderItems",
                principalColumns: new[] { "OrderId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FundingObligations_OrderServices_OrderId_OrderServiceId",
                schema: "Order",
                table: "FundingObligations",
                columns: new[] { "OrderId", "OrderServiceId" },
                principalSchema: "Order",
                principalTable: "OrderServices",
                principalColumns: new[] { "OrderId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FundingObligations_PricingLines_OrderId_PricingLineId",
                schema: "Order",
                table: "FundingObligations",
                columns: new[] { "OrderId", "PricingLineId" },
                principalSchema: "Order",
                principalTable: "PricingLines",
                principalColumns: new[] { "OrderId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemServiceLinks_OrderItems_OrderIdAtAssociation_OrderItemId",
                schema: "Order",
                table: "OrderItemServiceLinks",
                columns: new[] { "OrderIdAtAssociation", "OrderItemId" },
                principalSchema: "Order",
                principalTable: "OrderItems",
                principalColumns: new[] { "OrderId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemServiceLinks_OrderServices_OrderIdAtAssociation_OrderServiceId",
                schema: "Order",
                table: "OrderItemServiceLinks",
                columns: new[] { "OrderIdAtAssociation", "OrderServiceId" },
                principalSchema: "Order",
                principalTable: "OrderServices",
                principalColumns: new[] { "OrderId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderServices_OrderChanges_OrderId_CreatedByChangeId",
                schema: "Order",
                table: "OrderServices",
                columns: new[] { "OrderId", "CreatedByChangeId" },
                principalSchema: "Order",
                principalTable: "OrderChanges",
                principalColumns: new[] { "OrderId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PricingLines_OrderItems_OrderId_OrderItemId",
                schema: "Order",
                table: "PricingLines",
                columns: new[] { "OrderId", "OrderItemId" },
                principalSchema: "Order",
                principalTable: "OrderItems",
                principalColumns: new[] { "OrderId", "Id" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
