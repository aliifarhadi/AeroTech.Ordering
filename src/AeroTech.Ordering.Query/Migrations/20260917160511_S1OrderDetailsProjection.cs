using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ordering.Query.Migrations
{
    /// <inheritdoc />
    public partial class S1OrderDetailsProjection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ReadModel");

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                schema: "ReadModel",
                columns: table => new
                {
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    OwnerAirlineId = table.Column<long>(type: "bigint", nullable: false),
                    FinancialCustomerId = table.Column<long>(type: "bigint", nullable: false),
                    OrderReference = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OrderRevision = table.Column<long>(type: "bigint", nullable: false),
                    CommercialVersion = table.Column<int>(type: "int", nullable: false),
                    ProjectionSchemaVersion = table.Column<int>(type: "int", nullable: false),
                    DetailsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ProjectedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => x.OrderId);
                    table.CheckConstraint("CK_OrderDetails_Json", "ISJSON([DetailsJson]) = 1");
                    table.CheckConstraint("CK_OrderDetails_Revision", "[OrderRevision] >= 1 AND [ProjectionSchemaVersion] >= 1");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OwnerAirlineId_FinancialCustomerId_CreatedAt",
                schema: "ReadModel",
                table: "OrderDetails",
                columns: new[] { "OwnerAirlineId", "FinancialCustomerId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OwnerAirlineId_OrderReference",
                schema: "ReadModel",
                table: "OrderDetails",
                columns: new[] { "OwnerAirlineId", "OrderReference" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderDetails",
                schema: "ReadModel");
        }
    }
}
