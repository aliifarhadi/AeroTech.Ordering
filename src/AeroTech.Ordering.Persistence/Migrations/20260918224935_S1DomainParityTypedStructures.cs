using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ordering.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class S1DomainParityTypedStructures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiresFunding",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.AddColumn<int>(
                name: "CalculationKind",
                schema: "Order",
                table: "PricingLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConversionDecimalPlaces",
                schema: "Order",
                table: "PricingLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConversionFromCurrencyRef",
                schema: "Order",
                table: "PricingLines",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ConversionRate",
                schema: "Order",
                table: "PricingLines",
                type: "decimal(28,12)",
                precision: 28,
                scale: 12,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConversionRoundingToken",
                schema: "Order",
                table: "PricingLines",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConversionSourceRef",
                schema: "Order",
                table: "PricingLines",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConversionToCurrencyRef",
                schema: "Order",
                table: "PricingLines",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceCode",
                schema: "Order",
                table: "PricingLines",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceName",
                schema: "Order",
                table: "PricingLines",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceReference",
                schema: "Order",
                table: "PricingLines",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CapacityUnits",
                schema: "Order",
                table: "OrderServices",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryProviderRef",
                schema: "Order",
                table: "OrderServices",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FulfillmentProfileAssurance",
                schema: "Order",
                table: "OrderServices",
                type: "int",
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
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "Order",
                table: "OrderServices",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PriceTreatment",
                schema: "Order",
                table: "OrderServices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceCode",
                schema: "Order",
                table: "OrderServices",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SoldTermChangeable",
                schema: "Order",
                table: "OrderServices",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SoldTermRefundable",
                schema: "Order",
                table: "OrderServices",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SoldTermUpgradable",
                schema: "Order",
                table: "OrderServices",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierPartyRef",
                schema: "Order",
                table: "OrderServices",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AircraftRef",
                schema: "Order",
                table: "OrderSegments",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationTerminalRef",
                schema: "Order",
                table: "OrderSegments",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                schema: "Order",
                table: "OrderSegments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlightNumber",
                schema: "Order",
                table: "OrderSegments",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlightVersion",
                schema: "Order",
                table: "OrderSegments",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "JourneyId",
                schema: "Order",
                table: "OrderSegments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MarketingCarrierRef",
                schema: "Order",
                table: "OrderSegments",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatingCarrierRef",
                schema: "Order",
                table: "OrderSegments",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginTerminalRef",
                schema: "Order",
                table: "OrderSegments",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceCapacityRef",
                schema: "Order",
                table: "OrderSegments",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Arrival",
                schema: "Order",
                table: "OrderSegmentLegs",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Departure",
                schema: "Order",
                table: "OrderSegmentLegs",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationRef",
                schema: "Order",
                table: "OrderSegmentLegs",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationTerminalRef",
                schema: "Order",
                table: "OrderSegmentLegs",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginRef",
                schema: "Order",
                table: "OrderSegmentLegs",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginTerminalRef",
                schema: "Order",
                table: "OrderSegmentLegs",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "JourneyType",
                schema: "Order",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservedTicketingDeadlineSourceOwner",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservedTicketingDeadlineSourceRef",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ObservedTicketingDeadlineValue",
                schema: "Order",
                table: "Orders",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SaleCurrencyCode",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceJourneyTypeRaw",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductBrandCode",
                schema: "Order",
                table: "OrderItems",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductBrandName",
                schema: "Order",
                table: "OrderItems",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductCode",
                schema: "Order",
                table: "OrderItems",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                schema: "Order",
                table: "OrderItems",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductSourceOfferId",
                schema: "Order",
                table: "OrderItems",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductSourceOfferItemRef",
                schema: "Order",
                table: "OrderItems",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductSourceSystem",
                schema: "Order",
                table: "OrderItems",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductVersion",
                schema: "Order",
                table: "OrderItems",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TermsCapturedAt",
                schema: "Order",
                table: "OrderItems",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TermsChangeability",
                schema: "Order",
                table: "OrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TermsRefundability",
                schema: "Order",
                table: "OrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TermsSourcePolicyRef",
                schema: "Order",
                table: "OrderItems",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TermsSourcePolicyVersion",
                schema: "Order",
                table: "OrderItems",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TermsSourceSystem",
                schema: "Order",
                table: "OrderItems",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TermsUpgradeEligibility",
                schema: "Order",
                table: "OrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PricingUnitsJson",
                schema: "Order",
                table: "FareConstructions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "CabinBaggagePieces",
                schema: "Order",
                table: "AirTransportServiceDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CabinBaggageWeight",
                schema: "Order",
                table: "AirTransportServiceDetails",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CabinBaggageWeightUnit",
                schema: "Order",
                table: "AirTransportServiceDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CheckedBaggagePieces",
                schema: "Order",
                table: "AirTransportServiceDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CheckedBaggageWeight",
                schema: "Order",
                table: "AirTransportServiceDetails",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CheckedBaggageWeightUnit",
                schema: "Order",
                table: "AirTransportServiceDetails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FareConstructionItems",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    FareConstructionId = table.Column<long>(type: "bigint", nullable: false),
                    OrderItemId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FareConstructionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FareConstructionItems_FareConstructions_FareConstructionId",
                        column: x => x.FareConstructionId,
                        principalSchema: "Order",
                        principalTable: "FareConstructions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FareConstructionItems_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalSchema: "Order",
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FarePricingGroups",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    FareConstructionId = table.Column<long>(type: "bigint", nullable: false),
                    PassengerTypeCode = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarePricingGroups", x => x.Id);
                    table.CheckConstraint("CK_FarePricingGroups_Quantity", "[Quantity] >= 1");
                    table.ForeignKey(
                        name: "FK_FarePricingGroups_FareConstructions_FareConstructionId",
                        column: x => x.FareConstructionId,
                        principalSchema: "Order",
                        principalTable: "FareConstructions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItemServiceLinkSegments",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    LinkId = table.Column<long>(type: "bigint", nullable: false),
                    SegmentId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemServiceLinkSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItemServiceLinkSegments_OrderItemServiceLinks_LinkId",
                        column: x => x.LinkId,
                        principalSchema: "Order",
                        principalTable: "OrderItemServiceLinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemServiceLinkSegments_OrderSegments_SegmentId",
                        column: x => x.SegmentId,
                        principalSchema: "Order",
                        principalTable: "OrderSegments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItemServiceLinkTravelers",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    LinkId = table.Column<long>(type: "bigint", nullable: false),
                    TravelerId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemServiceLinkTravelers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItemServiceLinkTravelers_OrderItemServiceLinks_LinkId",
                        column: x => x.LinkId,
                        principalSchema: "Order",
                        principalTable: "OrderItemServiceLinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemServiceLinkTravelers_OrderTravelers_TravelerId",
                        column: x => x.TravelerId,
                        principalSchema: "Order",
                        principalTable: "OrderTravelers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderJourneys",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    SourceBoundRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    SourceDirectionRaw = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Direction = table.Column<int>(type: "int", nullable: true),
                    OriginRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DestinationRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderJourneys", x => x.Id);
                    table.CheckConstraint("CK_OrderJourneys_Sequence", "[Sequence] >= 1");
                    table.ForeignKey(
                        name: "FK_OrderJourneys_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "Order",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FarePricingGroupTravelers",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    PricingGroupId = table.Column<long>(type: "bigint", nullable: false),
                    TravelerId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarePricingGroupTravelers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FarePricingGroupTravelers_FarePricingGroups_PricingGroupId",
                        column: x => x.PricingGroupId,
                        principalSchema: "Order",
                        principalTable: "FarePricingGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FarePricingGroupTravelers_OrderTravelers_TravelerId",
                        column: x => x.TravelerId,
                        principalSchema: "Order",
                        principalTable: "OrderTravelers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FarePricingUnits",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    FareConstructionId = table.Column<long>(type: "bigint", nullable: false),
                    PricingGroupId = table.Column<long>(type: "bigint", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    SourceUnitRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SourceKindRaw = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CombinationMethod = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarePricingUnits", x => x.Id);
                    table.CheckConstraint("CK_FarePricingUnits_Sequence", "[Sequence] >= 1");
                    table.ForeignKey(
                        name: "FK_FarePricingUnits_FareConstructions_FareConstructionId",
                        column: x => x.FareConstructionId,
                        principalSchema: "Order",
                        principalTable: "FareConstructions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FarePricingUnits_FarePricingGroups_PricingGroupId",
                        column: x => x.PricingGroupId,
                        principalSchema: "Order",
                        principalTable: "FarePricingGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FareComponents",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    PricingUnitId = table.Column<long>(type: "bigint", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    SourceFareRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    FareBasis = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    FareFamily = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    FareType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CabinRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    RbdRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    BookingClass = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    TicketingRestrictionMinutes = table.Column<int>(type: "int", nullable: true),
                    FareOwnerRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    TariffRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    RuleRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    RoutingRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FareComponents", x => x.Id);
                    table.CheckConstraint("CK_FareComponents_Sequence", "[Sequence] >= 1");
                    table.ForeignKey(
                        name: "FK_FareComponents_FarePricingUnits_PricingUnitId",
                        column: x => x.PricingUnitId,
                        principalSchema: "Order",
                        principalTable: "FarePricingUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FarePricingUnitCoveredBounds",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    PricingUnitId = table.Column<long>(type: "bigint", nullable: false),
                    SourceBoundRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarePricingUnitCoveredBounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FarePricingUnitCoveredBounds_FarePricingUnits_PricingUnitId",
                        column: x => x.PricingUnitId,
                        principalSchema: "Order",
                        principalTable: "FarePricingUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FareComponentSegments",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    FareComponentId = table.Column<long>(type: "bigint", nullable: false),
                    OrderSegmentId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FareComponentSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FareComponentSegments_FareComponents_FareComponentId",
                        column: x => x.FareComponentId,
                        principalSchema: "Order",
                        principalTable: "FareComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FareComponentSegments_OrderSegments_OrderSegmentId",
                        column: x => x.OrderSegmentId,
                        principalSchema: "Order",
                        principalTable: "OrderSegments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FareComponentServices",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    FareComponentId = table.Column<long>(type: "bigint", nullable: false),
                    OrderServiceId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FareComponentServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FareComponentServices_FareComponents_FareComponentId",
                        column: x => x.FareComponentId,
                        principalSchema: "Order",
                        principalTable: "FareComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FareComponentServices_OrderServices_OrderServiceId",
                        column: x => x.OrderServiceId,
                        principalSchema: "Order",
                        principalTable: "OrderServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderServices_CapacityUnits",
                schema: "Order",
                table: "OrderServices",
                sql: "[CapacityUnits] IS NULL OR [CapacityUnits] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_OrderSegments_JourneyId",
                schema: "Order",
                table: "OrderSegments",
                column: "JourneyId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderSegments_Duration",
                schema: "Order",
                table: "OrderSegments",
                sql: "[Duration] IS NULL OR [Duration] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_FareComponents_PricingUnitId_Sequence",
                schema: "Order",
                table: "FareComponents",
                columns: new[] { "PricingUnitId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FareComponentSegments_FareComponentId_OrderSegmentId",
                schema: "Order",
                table: "FareComponentSegments",
                columns: new[] { "FareComponentId", "OrderSegmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FareComponentSegments_OrderSegmentId",
                schema: "Order",
                table: "FareComponentSegments",
                column: "OrderSegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_FareComponentServices_FareComponentId_OrderServiceId",
                schema: "Order",
                table: "FareComponentServices",
                columns: new[] { "FareComponentId", "OrderServiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FareComponentServices_OrderServiceId",
                schema: "Order",
                table: "FareComponentServices",
                column: "OrderServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_FareConstructionItems_FareConstructionId_OrderItemId",
                schema: "Order",
                table: "FareConstructionItems",
                columns: new[] { "FareConstructionId", "OrderItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FareConstructionItems_OrderItemId",
                schema: "Order",
                table: "FareConstructionItems",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_FarePricingGroups_FareConstructionId",
                schema: "Order",
                table: "FarePricingGroups",
                column: "FareConstructionId");

            migrationBuilder.CreateIndex(
                name: "IX_FarePricingGroupTravelers_PricingGroupId_TravelerId",
                schema: "Order",
                table: "FarePricingGroupTravelers",
                columns: new[] { "PricingGroupId", "TravelerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FarePricingGroupTravelers_TravelerId",
                schema: "Order",
                table: "FarePricingGroupTravelers",
                column: "TravelerId");

            migrationBuilder.CreateIndex(
                name: "IX_FarePricingUnitCoveredBounds_PricingUnitId_SourceBoundRef",
                schema: "Order",
                table: "FarePricingUnitCoveredBounds",
                columns: new[] { "PricingUnitId", "SourceBoundRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FarePricingUnits_FareConstructionId_SourceUnitRef",
                schema: "Order",
                table: "FarePricingUnits",
                columns: new[] { "FareConstructionId", "SourceUnitRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FarePricingUnits_PricingGroupId",
                schema: "Order",
                table: "FarePricingUnits",
                column: "PricingGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemServiceLinkSegments_LinkId_SegmentId",
                schema: "Order",
                table: "OrderItemServiceLinkSegments",
                columns: new[] { "LinkId", "SegmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemServiceLinkSegments_SegmentId",
                schema: "Order",
                table: "OrderItemServiceLinkSegments",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemServiceLinkTravelers_LinkId_TravelerId",
                schema: "Order",
                table: "OrderItemServiceLinkTravelers",
                columns: new[] { "LinkId", "TravelerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemServiceLinkTravelers_TravelerId",
                schema: "Order",
                table: "OrderItemServiceLinkTravelers",
                column: "TravelerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderJourneys_OrderId_Sequence",
                schema: "Order",
                table: "OrderJourneys",
                columns: new[] { "OrderId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderJourneys_OrderId_SourceBoundRef",
                schema: "Order",
                table: "OrderJourneys",
                columns: new[] { "OrderId", "SourceBoundRef" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderSegments_OrderJourneys_JourneyId",
                schema: "Order",
                table: "OrderSegments",
                column: "JourneyId",
                principalSchema: "Order",
                principalTable: "OrderJourneys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
UPDATE [Order].[PricingLines] SET [CalculationKind] = 1 WHERE [CalculationKind] IS NULL;

IF EXISTS (
    SELECT 1 FROM [Order].[OrderServices]
    WHERE [PriceTreatment] IS NULL
      AND [FulfillmentProfileRef] NOT IN (N'AIROFFER-OBSERVED-AIR-UNCERTIFIED', N'REFERENCE-AIR-ETKT'))
    THROW 51000, 'S1 parity migration: an accepted service carries no fulfillment profile whose price treatment is established.', 1;

UPDATE [Order].[OrderServices]
SET [PriceTreatment] = 4,
    [FulfillmentProfileVersion] = N'1',
    [FulfillmentProfileAssurance] = CASE WHEN [FulfillmentProfileRef] = N'REFERENCE-AIR-ETKT' THEN 1 ELSE 2 END,
    [FundingRequirement] = CASE WHEN [FulfillmentProfileRef] = N'REFERENCE-AIR-ETKT' THEN 3 ELSE 1 END,
    [ReservationRequirement] = CASE WHEN [FulfillmentProfileRef] = N'REFERENCE-AIR-ETKT' THEN [ReservationRequirement] ELSE 4 END,
    [DocumentKind] = CASE WHEN [FulfillmentProfileRef] = N'REFERENCE-AIR-ETKT' THEN [DocumentKind] ELSE 5 END,
    [CapacityUnits] = CASE WHEN [FulfillmentProfileRef] = N'REFERENCE-AIR-ETKT' THEN [CapacityUnits] ELSE NULL END
WHERE [PriceTreatment] IS NULL;

UPDATE item
SET item.[ProductSourceSystem] = [order].[AcceptedSourceOwner],
    item.[ProductSourceOfferId] = [order].[AcceptedSourceOfferId],
    item.[TermsSourceSystem] = [order].[AcceptedSourceOwner],
    item.[TermsCapturedAt] = [order].[SourceCapturedAt],
    item.[TermsRefundability] = 1,
    item.[TermsChangeability] = 1,
    item.[TermsUpgradeEligibility] = 1
FROM [Order].[OrderItems] item
INNER JOIN [Order].[Orders] [order] ON [order].[Id] = item.[OrderId]
WHERE item.[ProductSourceSystem] IS NULL;
");

            migrationBuilder.AlterColumn<int>(
                name: "CalculationKind",
                schema: "Order",
                table: "PricingLines",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FulfillmentProfileAssurance",
                schema: "Order",
                table: "OrderServices",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FulfillmentProfileVersion",
                schema: "Order",
                table: "OrderServices",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FundingRequirement",
                schema: "Order",
                table: "OrderServices",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PriceTreatment",
                schema: "Order",
                table: "OrderServices",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProductSourceOfferId",
                schema: "Order",
                table: "OrderItems",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(2048)",
                oldMaxLength: 2048,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProductSourceSystem",
                schema: "Order",
                table: "OrderItems",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "TermsCapturedAt",
                schema: "Order",
                table: "OrderItems",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TermsChangeability",
                schema: "Order",
                table: "OrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TermsRefundability",
                schema: "Order",
                table: "OrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TermsSourceSystem",
                schema: "Order",
                table: "OrderItems",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TermsUpgradeEligibility",
                schema: "Order",
                table: "OrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderSegments_OrderJourneys_JourneyId",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropTable(
                name: "FareComponentSegments",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "FareComponentServices",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "FareConstructionItems",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "FarePricingGroupTravelers",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "FarePricingUnitCoveredBounds",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "OrderItemServiceLinkSegments",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "OrderItemServiceLinkTravelers",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "OrderJourneys",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "FareComponents",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "FarePricingUnits",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "FarePricingGroups",
                schema: "Order");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderServices_CapacityUnits",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropIndex(
                name: "IX_OrderSegments_JourneyId",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderSegments_Duration",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropColumn(
                name: "CalculationKind",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "ConversionDecimalPlaces",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "ConversionFromCurrencyRef",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "ConversionRate",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "ConversionRoundingToken",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "ConversionSourceRef",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "ConversionToCurrencyRef",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "SourceCode",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "SourceName",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "SourceReference",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "DeliveryProviderRef",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "FulfillmentProfileAssurance",
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
                name: "Name",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "PriceTreatment",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "ServiceCode",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "SoldTermChangeable",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "SoldTermRefundable",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "SoldTermUpgradable",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "SupplierPartyRef",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "AircraftRef",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropColumn(
                name: "DestinationTerminalRef",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropColumn(
                name: "Duration",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropColumn(
                name: "FlightNumber",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropColumn(
                name: "FlightVersion",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropColumn(
                name: "JourneyId",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropColumn(
                name: "MarketingCarrierRef",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropColumn(
                name: "OperatingCarrierRef",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropColumn(
                name: "OriginTerminalRef",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropColumn(
                name: "SourceCapacityRef",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropColumn(
                name: "Arrival",
                schema: "Order",
                table: "OrderSegmentLegs");

            migrationBuilder.DropColumn(
                name: "Departure",
                schema: "Order",
                table: "OrderSegmentLegs");

            migrationBuilder.DropColumn(
                name: "DestinationRef",
                schema: "Order",
                table: "OrderSegmentLegs");

            migrationBuilder.DropColumn(
                name: "DestinationTerminalRef",
                schema: "Order",
                table: "OrderSegmentLegs");

            migrationBuilder.DropColumn(
                name: "OriginRef",
                schema: "Order",
                table: "OrderSegmentLegs");

            migrationBuilder.DropColumn(
                name: "OriginTerminalRef",
                schema: "Order",
                table: "OrderSegmentLegs");

            migrationBuilder.DropColumn(
                name: "JourneyType",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ObservedTicketingDeadlineSourceOwner",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ObservedTicketingDeadlineSourceRef",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ObservedTicketingDeadlineValue",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SaleCurrencyCode",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SourceJourneyTypeRaw",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ProductBrandCode",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductBrandName",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductCode",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductName",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductSourceOfferId",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductSourceOfferItemRef",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductSourceSystem",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductVersion",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "TermsCapturedAt",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "TermsChangeability",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "TermsRefundability",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "TermsSourcePolicyRef",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "TermsSourcePolicyVersion",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "TermsSourceSystem",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "TermsUpgradeEligibility",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "CabinBaggagePieces",
                schema: "Order",
                table: "AirTransportServiceDetails");

            migrationBuilder.DropColumn(
                name: "CabinBaggageWeight",
                schema: "Order",
                table: "AirTransportServiceDetails");

            migrationBuilder.DropColumn(
                name: "CabinBaggageWeightUnit",
                schema: "Order",
                table: "AirTransportServiceDetails");

            migrationBuilder.DropColumn(
                name: "CheckedBaggagePieces",
                schema: "Order",
                table: "AirTransportServiceDetails");

            migrationBuilder.DropColumn(
                name: "CheckedBaggageWeight",
                schema: "Order",
                table: "AirTransportServiceDetails");

            migrationBuilder.DropColumn(
                name: "CheckedBaggageWeightUnit",
                schema: "Order",
                table: "AirTransportServiceDetails");

            migrationBuilder.AlterColumn<int>(
                name: "CapacityUnits",
                schema: "Order",
                table: "OrderServices",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresFunding",
                schema: "Order",
                table: "OrderServices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "PricingUnitsJson",
                schema: "Order",
                table: "FareConstructions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
