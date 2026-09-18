using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ordering.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class S1OrderCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Order");

            migrationBuilder.EnsureSchema(
                name: "Operations");

            migrationBuilder.AddColumn<long>(
                name: "EventOrdinal",
                schema: "dbo",
                table: "OutboxMessages",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "StreamId",
                schema: "dbo",
                table: "OutboxMessages",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StreamKind",
                schema: "dbo",
                table: "OutboxMessages",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CommandReceipts",
                schema: "Operations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OwnerAirlineId = table.Column<long>(type: "bigint", nullable: false),
                    FinancialCustomerId = table.Column<long>(type: "bigint", nullable: false),
                    CallerScope = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CommandKind = table.Column<int>(type: "int", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CanonicalizationVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    RequestDigest = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    OperationId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PreparationId = table.Column<long>(type: "bigint", nullable: true),
                    OrderId = table.Column<long>(type: "bigint", nullable: true),
                    ResultJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandReceipts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderPreparations",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OwnerAirlineId = table.Column<long>(type: "bigint", nullable: false),
                    FinancialCustomerId = table.Column<long>(type: "bigint", nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    SellingOfficeId = table.Column<long>(type: "bigint", nullable: true),
                    CallerScope = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ActorContextType = table.Column<int>(type: "int", nullable: false),
                    ActorId = table.Column<long>(type: "bigint", nullable: true),
                    SourceOwner = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SourceOfferId = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    ProviderProfileId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ContractVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AcceptanceProfile = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AcceptanceAssurance = table.Column<int>(type: "int", nullable: false),
                    OwnerBindingRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    SourcePayloadHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CanonicalizationVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SnapshotDigest = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    CandidateJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PricedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CapturedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    OfferValidityState = table.Column<int>(type: "int", nullable: false),
                    OfferValidityValue = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    OfferValidityOwner = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OfferValiditySourceRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    OfferValidityReason = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    PriceValidityState = table.Column<int>(type: "int", nullable: false),
                    PriceValidityValue = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PriceValidityOwner = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PriceValiditySourceRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PriceValidityReason = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    TicketingValidityState = table.Column<int>(type: "int", nullable: false),
                    TicketingValidityValue = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TicketingValidityOwner = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TicketingValiditySourceRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    TicketingValidityReason = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ClientReference = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ConsumedByOrderId = table.Column<long>(type: "bigint", nullable: true),
                    ConsumedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderPreparations", x => x.Id);
                    table.CheckConstraint("CK_OrderPreparations_Consumption", "([ConsumedByOrderId] IS NULL AND [ConsumedAt] IS NULL) OR ([ConsumedByOrderId] IS NOT NULL AND [ConsumedAt] IS NOT NULL)");
                    table.CheckConstraint("CK_OrderPreparations_Digest", "LEN([SnapshotDigest]) = 64");
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OrderReference = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    RootOrderId = table.Column<long>(type: "bigint", nullable: false),
                    OwnerAirlineId = table.Column<long>(type: "bigint", nullable: false),
                    FinancialCustomerId = table.Column<long>(type: "bigint", nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    SellingOfficeId = table.Column<long>(type: "bigint", nullable: true),
                    BuyerActorContextType = table.Column<int>(type: "int", nullable: false),
                    BuyerActorId = table.Column<long>(type: "bigint", nullable: true),
                    SaleCurrencyRef = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    AcceptedPreparationId = table.Column<long>(type: "bigint", nullable: false),
                    AcceptedSourceOwner = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AcceptedSourceOfferId = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    AcceptedProviderProfileId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AcceptedContractVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AcceptanceProfile = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AcceptanceAssurance = table.Column<int>(type: "int", nullable: false),
                    AcceptedOwnerBindingRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    AcceptedSnapshotDigest = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    AcceptedSourcePayloadHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SourcePricedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SourceCapturedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ClientAcceptedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AcceptedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SourcePreparationId = table.Column<long>(type: "bigint", nullable: false),
                    OfferValidityState = table.Column<int>(type: "int", nullable: false),
                    OfferValidityValue = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    OfferValidityOwner = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OfferValiditySourceRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    OfferValidityReason = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    PriceValidityState = table.Column<int>(type: "int", nullable: false),
                    PriceValidityValue = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PriceValidityOwner = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PriceValiditySourceRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PriceValidityReason = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    TicketingValidityState = table.Column<int>(type: "int", nullable: false),
                    TicketingValidityValue = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TicketingValidityOwner = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TicketingValiditySourceRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    TicketingValidityReason = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CommercialSummary = table.Column<int>(type: "int", nullable: false),
                    CustomerTotalAmount = table.Column<decimal>(type: "decimal(28,8)", precision: 28, scale: 8, nullable: false),
                    CustomerTotalCurrencyRef = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CommercialVersion = table.Column<int>(type: "int", nullable: false),
                    FinancialSequence = table.Column<int>(type: "int", nullable: false),
                    OrderRevision = table.Column<long>(type: "bigint", nullable: false),
                    LastEventOrdinal = table.Column<long>(type: "bigint", nullable: false),
                    ClientReference = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.CheckConstraint("CK_Orders_Revisions", "[CommercialVersion] >= 1 AND [FinancialSequence] >= 0 AND [OrderRevision] >= 1 AND [LastEventOrdinal] >= 0");
                    table.CheckConstraint("CK_Orders_Root", "[RootOrderId] = [Id]");
                    table.ForeignKey(
                        name: "FK_Orders_OrderPreparations_SourcePreparationId",
                        column: x => x.SourcePreparationId,
                        principalSchema: "Order",
                        principalTable: "OrderPreparations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PreparationSourceEvidence",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    PreparationId = table.Column<long>(type: "bigint", nullable: false),
                    EvidenceRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PayloadHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CapturedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreparationSourceEvidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreparationSourceEvidence_OrderPreparations_PreparationId",
                        column: x => x.PreparationId,
                        principalSchema: "Order",
                        principalTable: "OrderPreparations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderChanges",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CommercialVersion = table.Column<int>(type: "int", nullable: false),
                    ActorContextType = table.Column<int>(type: "int", nullable: false),
                    ActorId = table.Column<long>(type: "bigint", nullable: true),
                    SourceDecisionRef = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CommittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderChanges_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "Order",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderContacts",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderContacts_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "Order",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderSegments",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    SourceSegmentRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    OriginRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DestinationRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SoldDeparture = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SoldArrival = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FlightRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderSegments", x => x.Id);
                    table.CheckConstraint("CK_OrderSegments_Sequence", "[Sequence] >= 1");
                    table.ForeignKey(
                        name: "FK_OrderSegments_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "Order",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderTravelers",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    SourceTravellerRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ClientTravelerRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PassengerTypeCode = table.Column<int>(type: "int", nullable: false),
                    InfantParentTravelerId = table.Column<long>(type: "bigint", nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTravelers", x => x.Id);
                    table.CheckConstraint("CK_OrderTravelers_Guardian", "[InfantParentTravelerId] IS NULL OR [InfantParentTravelerId] <> [Id]");
                    table.ForeignKey(
                        name: "FK_OrderTravelers_OrderTravelers_InfantParentTravelerId",
                        column: x => x.InfantParentTravelerId,
                        principalSchema: "Order",
                        principalTable: "OrderTravelers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderTravelers_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "Order",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FareConstructions",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OrderIdAtCreation = table.Column<long>(type: "bigint", nullable: false),
                    CreatedByChangeId = table.Column<long>(type: "bigint", nullable: false),
                    SupersededByConstructionId = table.Column<long>(type: "bigint", nullable: true),
                    Assurance = table.Column<int>(type: "int", nullable: false),
                    SourceContextRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PricingUnitsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FareConstructions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FareConstructions_FareConstructions_SupersededByConstructionId",
                        column: x => x.SupersededByConstructionId,
                        principalSchema: "Order",
                        principalTable: "FareConstructions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FareConstructions_OrderChanges_CreatedByChangeId",
                        column: x => x.CreatedByChangeId,
                        principalSchema: "Order",
                        principalTable: "OrderChanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FareConstructions_Orders_OrderIdAtCreation",
                        column: x => x.OrderIdAtCreation,
                        principalSchema: "Order",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    SourceItemRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    SourceOfferItemRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    AcceptedTotalAmount = table.Column<decimal>(type: "decimal(28,8)", precision: 28, scale: 8, nullable: false),
                    AcceptedTotalCurrencyRef = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CreatedByChangeId = table.Column<long>(type: "bigint", nullable: false),
                    CommercialStatus = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_OrderChanges_CreatedByChangeId",
                        column: x => x.CreatedByChangeId,
                        principalSchema: "Order",
                        principalTable: "OrderChanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "Order",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PriceChangeSets",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    ChangeId = table.Column<long>(type: "bigint", nullable: false),
                    FinancialSequence = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    SourceDecisionRef = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    BaseCommercialVersion = table.Column<int>(type: "int", nullable: false),
                    CommittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceChangeSets", x => x.Id);
                    table.CheckConstraint("CK_PriceChangeSets_Sequence", "[FinancialSequence] >= 1");
                    table.ForeignKey(
                        name: "FK_PriceChangeSets_OrderChanges_ChangeId",
                        column: x => x.ChangeId,
                        principalSchema: "Order",
                        principalTable: "OrderChanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PriceChangeSets_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "Order",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderSegmentLegs",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    SegmentId = table.Column<long>(type: "bigint", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    SourceLegRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderSegmentLegs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderSegmentLegs_OrderSegments_SegmentId",
                        column: x => x.SegmentId,
                        principalSchema: "Order",
                        principalTable: "OrderSegments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderTravelerIdentities",
                schema: "Order",
                columns: table => new
                {
                    TravelerId = table.Column<long>(type: "bigint", nullable: false),
                    GivenName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTravelerIdentities", x => x.TravelerId);
                    table.ForeignKey(
                        name: "FK_OrderTravelerIdentities_OrderTravelers_TravelerId",
                        column: x => x.TravelerId,
                        principalSchema: "Order",
                        principalTable: "OrderTravelers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FundingObligations",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Purpose = table.Column<int>(type: "int", nullable: false),
                    AmountAmount = table.Column<decimal>(type: "decimal(28,8)", precision: 28, scale: 8, nullable: false),
                    AmountCurrencyRef = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    OrderItemId = table.Column<long>(type: "bigint", nullable: true),
                    ChangeId = table.Column<long>(type: "bigint", nullable: false),
                    SourceDecisionRef = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SupersededObligationId = table.Column<long>(type: "bigint", nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingObligations", x => x.Id);
                    table.CheckConstraint("CK_FundingObligations_Amount", "[AmountAmount] >= 0");
                    table.CheckConstraint("CK_FundingObligations_Version", "[Version] >= 1");
                    table.ForeignKey(
                        name: "FK_FundingObligations_FundingObligations_SupersededObligationId",
                        column: x => x.SupersededObligationId,
                        principalSchema: "Order",
                        principalTable: "FundingObligations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FundingObligations_OrderChanges_ChangeId",
                        column: x => x.ChangeId,
                        principalSchema: "Order",
                        principalTable: "OrderChanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FundingObligations_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalSchema: "Order",
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FundingObligations_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "Order",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderServices",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    OrderItemId = table.Column<long>(type: "bigint", nullable: false),
                    SourceServiceRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CommercialStatus = table.Column<int>(type: "int", nullable: false),
                    ServiceVersion = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    QuantityUnit = table.Column<int>(type: "int", nullable: false),
                    DetailSchema = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DetailSchemaVersion = table.Column<int>(type: "int", nullable: false),
                    FulfillmentProfileRef = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ReservationRequirement = table.Column<int>(type: "int", nullable: false),
                    DocumentKind = table.Column<int>(type: "int", nullable: false),
                    RequiresFunding = table.Column<bool>(type: "bit", nullable: false),
                    CapacityUnits = table.Column<int>(type: "int", nullable: false),
                    CreatedByChangeId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderServices", x => x.Id);
                    table.CheckConstraint("CK_OrderServices_Quantity", "[Quantity] > 0");
                    table.CheckConstraint("CK_OrderServices_Version", "[ServiceVersion] >= 1");
                    table.ForeignKey(
                        name: "FK_OrderServices_OrderChanges_CreatedByChangeId",
                        column: x => x.CreatedByChangeId,
                        principalSchema: "Order",
                        principalTable: "OrderChanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderServices_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalSchema: "Order",
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderServices_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "Order",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PricingLines",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    PriceChangeSetId = table.Column<long>(type: "bigint", nullable: false),
                    OrderItemId = table.Column<long>(type: "bigint", nullable: true),
                    SourceLineRef = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    CandidateLineRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Component = table.Column<int>(type: "int", nullable: false),
                    Effect = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    OriginalValueAmount = table.Column<decimal>(type: "decimal(28,8)", precision: 28, scale: 8, nullable: false),
                    OriginalValueCurrencyRef = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SaleValueAmount = table.Column<decimal>(type: "decimal(28,8)", precision: 28, scale: 8, nullable: false),
                    SaleValueCurrencyRef = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    BasisType = table.Column<int>(type: "int", nullable: false),
                    BasisId = table.Column<long>(type: "bigint", nullable: true),
                    SourceBasisRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SourceConversionRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    OriginalPricingLineId = table.Column<long>(type: "bigint", nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingLines", x => x.Id);
                    table.CheckConstraint("CK_PricingLines_CommissionNotCustomer", "NOT ([Component] = 9 AND [Effect] = 1)");
                    table.CheckConstraint("CK_PricingLines_Direction", "[Direction] IN (1, 2)");
                    table.CheckConstraint("CK_PricingLines_OriginalMagnitude", "[OriginalValueAmount] >= 0");
                    table.CheckConstraint("CK_PricingLines_OtherInformational", "[Component] <> 11 OR [Effect] = 3");
                    table.CheckConstraint("CK_PricingLines_ReversalReference", "[Role] <> 2 OR [OriginalPricingLineId] IS NOT NULL");
                    table.CheckConstraint("CK_PricingLines_SaleMagnitude", "[SaleValueAmount] >= 0");
                    table.CheckConstraint("CK_PricingLines_TaxNotSettlement", "NOT ([Component] = 3 AND [Effect] = 2)");
                    table.ForeignKey(
                        name: "FK_PricingLines_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalSchema: "Order",
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PricingLines_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "Order",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PricingLines_PriceChangeSets_PriceChangeSetId",
                        column: x => x.PriceChangeSetId,
                        principalSchema: "Order",
                        principalTable: "PriceChangeSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PricingLines_PricingLines_OriginalPricingLineId",
                        column: x => x.OriginalPricingLineId,
                        principalSchema: "Order",
                        principalTable: "PricingLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AirTransportServiceDetails",
                schema: "Order",
                columns: table => new
                {
                    ServiceId = table.Column<long>(type: "bigint", nullable: false),
                    CabinRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    RbdRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    BookingClass = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    FlightNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    FlightVersion = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    MarketingCarrierRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    OperatingCarrierRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirTransportServiceDetails", x => x.ServiceId);
                    table.ForeignKey(
                        name: "FK_AirTransportServiceDetails_OrderServices_ServiceId",
                        column: x => x.ServiceId,
                        principalSchema: "Order",
                        principalTable: "OrderServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "OrderServiceBeneficiaries",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ServiceId = table.Column<long>(type: "bigint", nullable: false),
                    TravelerId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderServiceBeneficiaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderServiceBeneficiaries_OrderServices_ServiceId",
                        column: x => x.ServiceId,
                        principalSchema: "Order",
                        principalTable: "OrderServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderServiceBeneficiaries_OrderTravelers_TravelerId",
                        column: x => x.TravelerId,
                        principalSchema: "Order",
                        principalTable: "OrderTravelers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderServiceCoverage",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ServiceId = table.Column<long>(type: "bigint", nullable: false),
                    SegmentId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderServiceCoverage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderServiceCoverage_OrderSegments_SegmentId",
                        column: x => x.SegmentId,
                        principalSchema: "Order",
                        principalTable: "OrderSegments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderServiceCoverage_OrderServices_ServiceId",
                        column: x => x.ServiceId,
                        principalSchema: "Order",
                        principalTable: "OrderServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UX_OutboxMessages_Stream_EventOrdinal",
                schema: "dbo",
                table: "OutboxMessages",
                columns: new[] { "StreamKind", "StreamId", "EventOrdinal" },
                unique: true,
                filter: "[StreamKind] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OutboxMessages_Stream",
                schema: "dbo",
                table: "OutboxMessages",
                sql: "([StreamKind] IS NULL AND [StreamId] IS NULL AND [EventOrdinal] IS NULL) OR ([StreamKind] IS NOT NULL AND [StreamId] IS NOT NULL AND [EventOrdinal] >= 1)");

            migrationBuilder.CreateIndex(
                name: "IX_CommandReceipts_OperationId",
                schema: "Operations",
                table: "CommandReceipts",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommandReceipts_OwnerAirlineId_OrderId",
                schema: "Operations",
                table: "CommandReceipts",
                columns: new[] { "OwnerAirlineId", "OrderId" });

            migrationBuilder.CreateIndex(
                name: "UX_CommandReceipts_Scope_Key",
                schema: "Operations",
                table: "CommandReceipts",
                columns: new[] { "OwnerAirlineId", "CallerScope", "CommandKind", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FareConstructions_CreatedByChangeId",
                schema: "Order",
                table: "FareConstructions",
                column: "CreatedByChangeId");

            migrationBuilder.CreateIndex(
                name: "IX_FareConstructions_SupersededByConstructionId",
                schema: "Order",
                table: "FareConstructions",
                column: "SupersededByConstructionId");

            migrationBuilder.CreateIndex(
                name: "UX_FareConstructions_CurrentPerOrder",
                schema: "Order",
                table: "FareConstructions",
                column: "OrderIdAtCreation",
                unique: true,
                filter: "[SupersededByConstructionId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FundingObligations_ChangeId",
                schema: "Order",
                table: "FundingObligations",
                column: "ChangeId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingObligations_OrderId_Id_Version",
                schema: "Order",
                table: "FundingObligations",
                columns: new[] { "OrderId", "Id", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FundingObligations_OrderItemId",
                schema: "Order",
                table: "FundingObligations",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingObligations_SupersededObligationId",
                schema: "Order",
                table: "FundingObligations",
                column: "SupersededObligationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderChanges_OrderId_CommercialVersion",
                schema: "Order",
                table: "OrderChanges",
                columns: new[] { "OrderId", "CommercialVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderContacts_OrderId_Sequence",
                schema: "Order",
                table: "OrderContacts",
                columns: new[] { "OrderId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_CreatedByChangeId",
                schema: "Order",
                table: "OrderItems",
                column: "CreatedByChangeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId_CommercialStatus",
                schema: "Order",
                table: "OrderItems",
                columns: new[] { "OrderId", "CommercialStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId_SourceItemRef",
                schema: "Order",
                table: "OrderItems",
                columns: new[] { "OrderId", "SourceItemRef" },
                unique: true);

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
                name: "IX_OrderItemServiceLinks_OrderItemId_OrderServiceId_LinkedByChangeId",
                schema: "Order",
                table: "OrderItemServiceLinks",
                columns: new[] { "OrderItemId", "OrderServiceId", "LinkedByChangeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemServiceLinks_OrderServiceId",
                schema: "Order",
                table: "OrderItemServiceLinks",
                column: "OrderServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderPreparations_ConsumedAt_CreatedAt",
                schema: "Order",
                table: "OrderPreparations",
                columns: new[] { "ConsumedAt", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderPreparations_OwnerAirlineId_FinancialCustomerId_CreatedAt",
                schema: "Order",
                table: "OrderPreparations",
                columns: new[] { "OwnerAirlineId", "FinancialCustomerId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_OrderPreparations_ConsumedByOrderId",
                schema: "Order",
                table: "OrderPreparations",
                column: "ConsumedByOrderId",
                unique: true,
                filter: "[ConsumedByOrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OwnerAirlineId_CommercialSummary",
                schema: "Order",
                table: "Orders",
                columns: new[] { "OwnerAirlineId", "CommercialSummary" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OwnerAirlineId_FinancialCustomerId_CreatedAt",
                schema: "Order",
                table: "Orders",
                columns: new[] { "OwnerAirlineId", "FinancialCustomerId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SourcePreparationId",
                schema: "Order",
                table: "Orders",
                column: "SourcePreparationId");

            migrationBuilder.CreateIndex(
                name: "UX_Orders_Owner_Reference",
                schema: "Order",
                table: "Orders",
                columns: new[] { "OwnerAirlineId", "OrderReference" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Orders_Owner_SourcePreparation",
                schema: "Order",
                table: "Orders",
                columns: new[] { "OwnerAirlineId", "SourcePreparationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderSegmentLegs_SegmentId_Sequence",
                schema: "Order",
                table: "OrderSegmentLegs",
                columns: new[] { "SegmentId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderSegments_OrderId_Sequence",
                schema: "Order",
                table: "OrderSegments",
                columns: new[] { "OrderId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderSegments_OrderId_SourceSegmentRef",
                schema: "Order",
                table: "OrderSegments",
                columns: new[] { "OrderId", "SourceSegmentRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderServiceBeneficiaries_ServiceId_TravelerId",
                schema: "Order",
                table: "OrderServiceBeneficiaries",
                columns: new[] { "ServiceId", "TravelerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderServiceBeneficiaries_TravelerId",
                schema: "Order",
                table: "OrderServiceBeneficiaries",
                column: "TravelerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderServiceCoverage_SegmentId",
                schema: "Order",
                table: "OrderServiceCoverage",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderServiceCoverage_ServiceId_SegmentId",
                schema: "Order",
                table: "OrderServiceCoverage",
                columns: new[] { "ServiceId", "SegmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderServices_CreatedByChangeId",
                schema: "Order",
                table: "OrderServices",
                column: "CreatedByChangeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderServices_OrderId_SourceServiceRef",
                schema: "Order",
                table: "OrderServices",
                columns: new[] { "OrderId", "SourceServiceRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderServices_OrderItemId_CommercialStatus",
                schema: "Order",
                table: "OrderServices",
                columns: new[] { "OrderItemId", "CommercialStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderTravelers_InfantParentTravelerId",
                schema: "Order",
                table: "OrderTravelers",
                column: "InfantParentTravelerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTravelers_OrderId_ClientTravelerRef",
                schema: "Order",
                table: "OrderTravelers",
                columns: new[] { "OrderId", "ClientTravelerRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderTravelers_OrderId_SourceTravellerRef",
                schema: "Order",
                table: "OrderTravelers",
                columns: new[] { "OrderId", "SourceTravellerRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreparationSourceEvidence_PreparationId_EvidenceRef",
                schema: "Order",
                table: "PreparationSourceEvidence",
                columns: new[] { "PreparationId", "EvidenceRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceChangeSets_ChangeId",
                schema: "Order",
                table: "PriceChangeSets",
                column: "ChangeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceChangeSets_OrderId_FinancialSequence",
                schema: "Order",
                table: "PriceChangeSets",
                columns: new[] { "OrderId", "FinancialSequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PricingLines_OrderId_OrderItemId",
                schema: "Order",
                table: "PricingLines",
                columns: new[] { "OrderId", "OrderItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_PricingLines_OrderItemId",
                schema: "Order",
                table: "PricingLines",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PricingLines_OriginalPricingLineId",
                schema: "Order",
                table: "PricingLines",
                column: "OriginalPricingLineId");

            migrationBuilder.CreateIndex(
                name: "IX_PricingLines_PriceChangeSetId_CandidateLineRef",
                schema: "Order",
                table: "PricingLines",
                columns: new[] { "PriceChangeSetId", "CandidateLineRef" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AirTransportServiceDetails",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "CommandReceipts",
                schema: "Operations");

            migrationBuilder.DropTable(
                name: "FareConstructions",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "FundingObligations",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "OrderContacts",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "OrderItemServiceLinks",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "OrderSegmentLegs",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "OrderServiceBeneficiaries",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "OrderServiceCoverage",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "OrderTravelerIdentities",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "PreparationSourceEvidence",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "PricingLines",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "OrderSegments",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "OrderServices",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "OrderTravelers",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "PriceChangeSets",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "OrderItems",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "OrderChanges",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "Orders",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "OrderPreparations",
                schema: "Order");

            migrationBuilder.DropIndex(
                name: "UX_OutboxMessages_Stream_EventOrdinal",
                schema: "dbo",
                table: "OutboxMessages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OutboxMessages_Stream",
                schema: "dbo",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "EventOrdinal",
                schema: "dbo",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "StreamId",
                schema: "dbo",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "StreamKind",
                schema: "dbo",
                table: "OutboxMessages");
        }
    }
}
