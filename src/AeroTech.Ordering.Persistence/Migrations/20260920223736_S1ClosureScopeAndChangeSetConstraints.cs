using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ordering.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class S1ClosureScopeAndChangeSetConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderServices_OrderSegments_SegmentId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderServices_OrderTravellers_TravellerId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropIndex(
                name: "IX_PriceChangeSets_ChangeId",
                schema: "Order",
                table: "PriceChangeSets");

            migrationBuilder.DropIndex(
                name: "IX_OrderServices_SegmentId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropIndex(
                name: "IX_OrderServices_TravellerId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropIndex(
                name: "IX_OrderSegments_OrderId",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_OrderTravellers_OrderId_Id",
                schema: "Order",
                table: "OrderTravellers",
                columns: new[] { "OrderId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_OrderSegments_OrderId_Id",
                schema: "Order",
                table: "OrderSegments",
                columns: new[] { "OrderId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_PriceChangeSets_ChangeId",
                schema: "Order",
                table: "PriceChangeSets",
                column: "ChangeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderServices_OrderId_SegmentId",
                schema: "Order",
                table: "OrderServices",
                columns: new[] { "OrderId", "SegmentId" });

            migrationBuilder.AddForeignKey(
                name: "FK_OrderServices_OrderSegments_OrderId_SegmentId",
                schema: "Order",
                table: "OrderServices",
                columns: new[] { "OrderId", "SegmentId" },
                principalSchema: "Order",
                principalTable: "OrderSegments",
                principalColumns: new[] { "OrderId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderServices_OrderTravellers_OrderId_TravellerId",
                schema: "Order",
                table: "OrderServices",
                columns: new[] { "OrderId", "TravellerId" },
                principalSchema: "Order",
                principalTable: "OrderTravellers",
                principalColumns: new[] { "OrderId", "Id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderServices_OrderSegments_OrderId_SegmentId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderServices_OrderTravellers_OrderId_TravellerId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropIndex(
                name: "IX_PriceChangeSets_ChangeId",
                schema: "Order",
                table: "PriceChangeSets");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_OrderTravellers_OrderId_Id",
                schema: "Order",
                table: "OrderTravellers");

            migrationBuilder.DropIndex(
                name: "IX_OrderServices_OrderId_SegmentId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_OrderSegments_OrderId_Id",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.CreateIndex(
                name: "IX_PriceChangeSets_ChangeId",
                schema: "Order",
                table: "PriceChangeSets",
                column: "ChangeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderServices_SegmentId",
                schema: "Order",
                table: "OrderServices",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderServices_TravellerId",
                schema: "Order",
                table: "OrderServices",
                column: "TravellerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderSegments_OrderId",
                schema: "Order",
                table: "OrderSegments",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderServices_OrderSegments_SegmentId",
                schema: "Order",
                table: "OrderServices",
                column: "SegmentId",
                principalSchema: "Order",
                principalTable: "OrderSegments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderServices_OrderTravellers_TravellerId",
                schema: "Order",
                table: "OrderServices",
                column: "TravellerId",
                principalSchema: "Order",
                principalTable: "OrderTravellers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
