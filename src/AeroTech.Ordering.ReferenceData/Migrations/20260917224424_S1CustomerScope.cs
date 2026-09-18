using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ordering.ReferenceData.Migrations
{
    /// <inheritdoc />
    public partial class S1CustomerScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CityId",
                schema: "ReferenceData",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "ReferenceData",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "ReferenceData",
                table: "Customers");

            migrationBuilder.RenameColumn(
                name: "UniqueIdentifier",
                schema: "ReferenceData",
                table: "Customers",
                newName: "CustomerNumber");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                schema: "ReferenceData",
                table: "Customers");

            migrationBuilder.AddColumn<string>(
                name: "SubjectName",
                schema: "ReferenceData",
                table: "Customers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.RenameIndex(
                name: "IX_Customers_UniqueIdentifier",
                schema: "ReferenceData",
                table: "Customers",
                newName: "IX_Customers_CustomerNumber");

            migrationBuilder.AlterColumn<int>(
                name: "PreferredCurrencyId",
                schema: "ReferenceData",
                table: "Customers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<long>(
                name: "SubjectId",
                schema: "ReferenceData",
                table: "Customers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "TravelAgencyId",
                schema: "ReferenceData",
                table: "Customers",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TravelAgencyId",
                schema: "ReferenceData",
                table: "Customers",
                column: "TravelAgencyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_TravelAgencyId",
                schema: "ReferenceData",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                schema: "ReferenceData",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "TravelAgencyId",
                schema: "ReferenceData",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SubjectName",
                schema: "ReferenceData",
                table: "Customers");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                schema: "ReferenceData",
                table: "Customers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "CustomerNumber",
                schema: "ReferenceData",
                table: "Customers",
                newName: "UniqueIdentifier");

            migrationBuilder.RenameIndex(
                name: "IX_Customers_CustomerNumber",
                schema: "ReferenceData",
                table: "Customers",
                newName: "IX_Customers_UniqueIdentifier");

            migrationBuilder.AlterColumn<int>(
                name: "PreferredCurrencyId",
                schema: "ReferenceData",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                schema: "ReferenceData",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "ReferenceData",
                table: "Customers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "ReferenceData",
                table: "Customers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }
    }
}
