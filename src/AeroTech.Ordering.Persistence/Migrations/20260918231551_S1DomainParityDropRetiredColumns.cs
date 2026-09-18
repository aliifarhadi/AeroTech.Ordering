using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ordering.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class S1DomainParityDropRetiredColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PricingUnitsJson",
                schema: "Order",
                table: "FareConstructions");

            migrationBuilder.DropColumn(
                name: "FlightNumber",
                schema: "Order",
                table: "AirTransportServiceDetails");

            migrationBuilder.DropColumn(
                name: "FlightVersion",
                schema: "Order",
                table: "AirTransportServiceDetails");

            migrationBuilder.DropColumn(
                name: "MarketingCarrierRef",
                schema: "Order",
                table: "AirTransportServiceDetails");

            migrationBuilder.DropColumn(
                name: "OperatingCarrierRef",
                schema: "Order",
                table: "AirTransportServiceDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PricingUnitsJson",
                schema: "Order",
                table: "FareConstructions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlightNumber",
                schema: "Order",
                table: "AirTransportServiceDetails",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlightVersion",
                schema: "Order",
                table: "AirTransportServiceDetails",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MarketingCarrierRef",
                schema: "Order",
                table: "AirTransportServiceDetails",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatingCarrierRef",
                schema: "Order",
                table: "AirTransportServiceDetails",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);
        }
    }
}
