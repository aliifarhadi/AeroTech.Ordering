using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ordering.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class S1AuthoritativeFinalShape : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FareConstructions_OrderChanges_CreatedByChangeId",
                schema: "Order",
                table: "FareConstructions");

            migrationBuilder.DropForeignKey(
                name: "FK_FundingObligations_OrderChanges_ChangeId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropForeignKey(
                name: "FK_FundingObligations_OrderItems_OrderItemId",
                schema: "Order",
                table: "FundingObligations");

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

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_OrderChanges_CreatedByChangeId",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemServiceLinks_OrderChanges_LinkedByChangeId",
                schema: "Order",
                table: "OrderItemServiceLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemServiceLinks_OrderItems_OrderItemId",
                schema: "Order",
                table: "OrderItemServiceLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemServiceLinks_OrderServices_OrderServiceId",
                schema: "Order",
                table: "OrderItemServiceLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OrderPreparations_SourcePreparationId",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderSegments_OrderJourneys_JourneyId",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderServices_OrderChanges_CreatedByChangeId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderServices_OrderItems_OrderItemId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderTravellers_OrderTravellers_InfantParentTravellerId",
                schema: "Order",
                table: "OrderTravellers");

            migrationBuilder.DropForeignKey(
                name: "FK_PriceChangeSets_OrderChanges_ChangeId",
                schema: "Order",
                table: "PriceChangeSets");

            migrationBuilder.DropForeignKey(
                name: "FK_PricingLines_OrderItems_OrderItemId",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropForeignKey(
                name: "FK_PricingLines_PriceChangeSets_PriceChangeSetId",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropIndex(
                name: "IX_PricingLines_OrderItemId",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropIndex(
                name: "IX_OrderTravellers_InfantParentTravellerId",
                schema: "Order",
                table: "OrderTravellers");

            migrationBuilder.DropIndex(
                name: "IX_OrderServices_CreatedByChangeId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropIndex(
                name: "IX_OrderServices_OrderId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropIndex(
                name: "IX_Orders_SourcePreparationId",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderItemServiceLinks_LinkedByChangeId",
                schema: "Order",
                table: "OrderItemServiceLinks");

            migrationBuilder.DropIndex(
                name: "IX_OrderItemServiceLinks_OrderIdAtAssociation",
                schema: "Order",
                table: "OrderItemServiceLinks");

            migrationBuilder.DropIndex(
                name: "IX_OrderItemServiceLinks_OrderServiceId",
                schema: "Order",
                table: "OrderItemServiceLinks");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_CreatedByChangeId",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderChanges_OrderId_CommercialVersion",
                schema: "Order",
                table: "OrderChanges");

            migrationBuilder.DropIndex(
                name: "IX_FundingObligations_ChangeId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropIndex(
                name: "IX_FundingObligations_OrderItemId",
                schema: "Order",
                table: "FundingObligations");

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

            migrationBuilder.DropIndex(
                name: "IX_FareConstructions_CreatedByChangeId",
                schema: "Order",
                table: "FareConstructions");

            migrationBuilder.AlterColumn<int>(
                name: "OriginAirportId",
                schema: "Order",
                table: "OrderSegmentLegs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DestinationAirportId",
                schema: "Order",
                table: "OrderSegmentLegs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DepartureDateTime",
                schema: "Order",
                table: "OrderSegmentLegs",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ArrivalDateTime",
                schema: "Order",
                table: "OrderSegmentLegs",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_PricingLines_OrderId_Id",
                schema: "Order",
                table: "PricingLines",
                columns: new[] { "OrderId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_PriceChangeSets_OrderId_Id",
                schema: "Order",
                table: "PriceChangeSets",
                columns: new[] { "OrderId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_OrderServices_OrderId_Id",
                schema: "Order",
                table: "OrderServices",
                columns: new[] { "OrderId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Orders_OwnerAirlineId_Id",
                schema: "Order",
                table: "Orders",
                columns: new[] { "OwnerAirlineId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_OrderPreparations_OwnerAirlineId_Id",
                schema: "Order",
                table: "OrderPreparations",
                columns: new[] { "OwnerAirlineId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_OrderJourneys_OrderId_Id",
                schema: "Order",
                table: "OrderJourneys",
                columns: new[] { "OrderId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_OrderItems_OrderId_Id",
                schema: "Order",
                table: "OrderItems",
                columns: new[] { "OrderId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_OrderChanges_OrderId_Id",
                schema: "Order",
                table: "OrderChanges",
                columns: new[] { "OrderId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_PricingLines_OrderId_PriceChangeSetId",
                schema: "Order",
                table: "PricingLines",
                columns: new[] { "OrderId", "PriceChangeSetId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_PricingLines_BasisType_Enum",
                schema: "Order",
                table: "PricingLines",
                sql: "[BasisType] IN (1, 2, 3, 4, 5, 6, 7, 8)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PricingLines_CalculationKind_Enum",
                schema: "Order",
                table: "PricingLines",
                sql: "[CalculationKind] IN (1, 2, 3)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PricingLines_Component_Enum",
                schema: "Order",
                table: "PricingLines",
                sql: "[Component] IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PricingLines_Direction_Enum",
                schema: "Order",
                table: "PricingLines",
                sql: "[Direction] IN (1, 2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PricingLines_Effect_Enum",
                schema: "Order",
                table: "PricingLines",
                sql: "[Effect] IN (1, 2, 3)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PricingLines_Role_Enum",
                schema: "Order",
                table: "PricingLines",
                sql: "[Role] IN (1, 2, 3, 4)");

            migrationBuilder.CreateIndex(
                name: "IX_PriceChangeSets_OrderId_ChangeId",
                schema: "Order",
                table: "PriceChangeSets",
                columns: new[] { "OrderId", "ChangeId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_PriceChangeSets_Reason_Enum",
                schema: "Order",
                table: "PriceChangeSets",
                sql: "[Reason] IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12)");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTravellers_OrderId_InfantParentTravellerId",
                schema: "Order",
                table: "OrderTravellers",
                columns: new[] { "OrderId", "InfantParentTravellerId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderTravellers_PassengerTypeCode_Enum",
                schema: "Order",
                table: "OrderTravellers",
                sql: "[PassengerTypeCode] IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116)");

            migrationBuilder.CreateIndex(
                name: "IX_OrderServices_OrderId_CreatedByChangeId",
                schema: "Order",
                table: "OrderServices",
                columns: new[] { "OrderId", "CreatedByChangeId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderServices_OrderId_OrderItemId",
                schema: "Order",
                table: "OrderServices",
                columns: new[] { "OrderId", "OrderItemId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderServices_CabinBaggageWeightUnit_Enum",
                schema: "Order",
                table: "OrderServices",
                sql: "[CabinBaggageWeightUnit] IS NULL OR [CabinBaggageWeightUnit] IN (1, 2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderServices_CheckedBaggageWeightUnit_Enum",
                schema: "Order",
                table: "OrderServices",
                sql: "[CheckedBaggageWeightUnit] IS NULL OR [CheckedBaggageWeightUnit] IN (1, 2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderServices_CommercialStatus_Enum",
                schema: "Order",
                table: "OrderServices",
                sql: "[CommercialStatus] IN (1, 2, 3, 4, 5)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderServices_DocumentAuthority_Enum",
                schema: "Order",
                table: "OrderServices",
                sql: "[DocumentAuthority] IS NULL OR [DocumentAuthority] IN (1, 2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderServices_FulfillmentDocumentKind_Enum",
                schema: "Order",
                table: "OrderServices",
                sql: "[FulfillmentDocumentKind] IN (1, 2, 3, 4, 5)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderServices_FulfillmentProfileAssurance_Enum",
                schema: "Order",
                table: "OrderServices",
                sql: "[FulfillmentProfileAssurance] IN (1, 2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderServices_FundingRequirement_Enum",
                schema: "Order",
                table: "OrderServices",
                sql: "[FundingRequirement] IN (1, 2, 3)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderServices_ReservationRequirement_Enum",
                schema: "Order",
                table: "OrderServices",
                sql: "[ReservationRequirement] IN (1, 2, 3, 4)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderServices_ServiceType_Enum",
                schema: "Order",
                table: "OrderServices",
                sql: "[ServiceType] IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22)");

            migrationBuilder.CreateIndex(
                name: "IX_OrderSegments_OrderId_JourneyId",
                schema: "Order",
                table: "OrderSegments",
                columns: new[] { "OrderId", "JourneyId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderSegments_Kind_Enum",
                schema: "Order",
                table: "OrderSegments",
                sql: "[Kind] IN (1, 2, 3)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_BuyerContextType_Enum",
                schema: "Order",
                table: "Orders",
                sql: "[BuyerContextType] IS NULL OR [BuyerContextType] IN (1, 2, 3, 4, 5, 6)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_Channel_Enum",
                schema: "Order",
                table: "Orders",
                sql: "[Channel] IN (1, 2, 3, 4, 5, 99)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_CommercialSummary_Enum",
                schema: "Order",
                table: "Orders",
                sql: "[CommercialSummary] IN (1, 2, 3, 4, 5)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_InitiatingActorContextType_Enum",
                schema: "Order",
                table: "Orders",
                sql: "[InitiatingActorContextType] IN (1, 2, 3, 4, 5, 6)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_JourneyType_Enum",
                schema: "Order",
                table: "Orders",
                sql: "[JourneyType] IN (1, 2, 3, 4)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_SellerContextType_Enum",
                schema: "Order",
                table: "Orders",
                sql: "[SellerContextType] IS NULL OR [SellerContextType] IN (1, 2, 3, 4, 5, 6)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_SellingOfficeKind_Enum",
                schema: "Order",
                table: "Orders",
                sql: "[SellingOfficeKind] IS NULL OR [SellingOfficeKind] IN (1, 2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderPreparations_AcceptanceAssurance_Enum",
                schema: "Order",
                table: "OrderPreparations",
                sql: "[AcceptanceAssurance] IN (1, 2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderJourneys_Direction_Enum",
                schema: "Order",
                table: "OrderJourneys",
                sql: "[Direction] IN (1, 2)");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemServiceLinks_OrderIdAtAssociation_LinkedByChangeId",
                schema: "Order",
                table: "OrderItemServiceLinks",
                columns: new[] { "OrderIdAtAssociation", "LinkedByChangeId" });

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
                name: "IX_OrderItems_OrderId_CreatedByChangeId",
                schema: "Order",
                table: "OrderItems",
                columns: new[] { "OrderId", "CreatedByChangeId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_CommercialStatus_Enum",
                schema: "Order",
                table: "OrderItems",
                sql: "[CommercialStatus] IN (1, 2, 3, 4, 5, 6, 7)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_Kind_Enum",
                schema: "Order",
                table: "OrderItems",
                sql: "[Kind] IN (1, 2, 3)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderContacts_Role_Enum",
                schema: "Order",
                table: "OrderContacts",
                sql: "[Role] IN (1, 2, 3, 4, 5)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderComponentTotals_Component_Enum",
                schema: "Order",
                table: "OrderComponentTotals",
                sql: "[Component] IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderComponentTotals_Effect_Enum",
                schema: "Order",
                table: "OrderComponentTotals",
                sql: "[Effect] IN (1, 2, 3)");

            migrationBuilder.CreateIndex(
                name: "IX_OrderChanges_OrderId_CommercialVersion",
                schema: "Order",
                table: "OrderChanges",
                columns: new[] { "OrderId", "CommercialVersion" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderChanges_ActorContextType_Enum",
                schema: "Order",
                table: "OrderChanges",
                sql: "[ActorContextType] IN (1, 2, 3, 4, 5, 6)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderChanges_Type_Enum",
                schema: "Order",
                table: "OrderChanges",
                sql: "[Type] IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14)");

            migrationBuilder.CreateIndex(
                name: "IX_FundingObligations_OrderId_ChangeId",
                schema: "Order",
                table: "FundingObligations",
                columns: new[] { "OrderId", "ChangeId" });

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
                name: "IX_FundingObligations_OrderId_PriceChangeSetId",
                schema: "Order",
                table: "FundingObligations",
                columns: new[] { "OrderId", "PriceChangeSetId" });

            migrationBuilder.CreateIndex(
                name: "IX_FundingObligations_OrderId_PricingLineId",
                schema: "Order",
                table: "FundingObligations",
                columns: new[] { "OrderId", "PricingLineId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_FundingObligations_Purpose_Enum",
                schema: "Order",
                table: "FundingObligations",
                sql: "[Purpose] IN (1, 2, 3, 4, 5)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FarePricingUnits_SourceConstructionType_Enum",
                schema: "Order",
                table: "FarePricingUnits",
                sql: "[SourceConstructionType] IN (1, 2, 3, 4, 5, 6, 7, 8)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FarePricingUnits_Type_Enum",
                schema: "Order",
                table: "FarePricingUnits",
                sql: "[Type] IN (1, 2, 3, 4, 5, 6)");

            migrationBuilder.CreateIndex(
                name: "IX_FareConstructions_OrderId_CreatedByChangeId",
                schema: "Order",
                table: "FareConstructions",
                columns: new[] { "OrderId", "CreatedByChangeId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_FareConstructions_Assurance_Enum",
                schema: "Order",
                table: "FareConstructions",
                sql: "[Assurance] IN (1, 2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CommandReceipts_CommandKind_Enum",
                schema: "Operations",
                table: "CommandReceipts",
                sql: "[CommandKind] IN (2, 3)");

            migrationBuilder.AddForeignKey(
                name: "FK_CommandReceipts_Orders_OwnerAirlineId_OrderId",
                schema: "Operations",
                table: "CommandReceipts",
                columns: new[] { "OwnerAirlineId", "OrderId" },
                principalSchema: "Order",
                principalTable: "Orders",
                principalColumns: new[] { "OwnerAirlineId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FareConstructions_OrderChanges_OrderId_CreatedByChangeId",
                schema: "Order",
                table: "FareConstructions",
                columns: new[] { "OrderId", "CreatedByChangeId" },
                principalSchema: "Order",
                principalTable: "OrderChanges",
                principalColumns: new[] { "OrderId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FundingObligations_OrderChanges_OrderId_ChangeId",
                schema: "Order",
                table: "FundingObligations",
                columns: new[] { "OrderId", "ChangeId" },
                principalSchema: "Order",
                principalTable: "OrderChanges",
                principalColumns: new[] { "OrderId", "Id" },
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_FundingObligations_PriceChangeSets_OrderId_PriceChangeSetId",
                schema: "Order",
                table: "FundingObligations",
                columns: new[] { "OrderId", "PriceChangeSetId" },
                principalSchema: "Order",
                principalTable: "PriceChangeSets",
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
                name: "FK_OrderItems_OrderChanges_OrderId_CreatedByChangeId",
                schema: "Order",
                table: "OrderItems",
                columns: new[] { "OrderId", "CreatedByChangeId" },
                principalSchema: "Order",
                principalTable: "OrderChanges",
                principalColumns: new[] { "OrderId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemServiceLinks_OrderChanges_OrderIdAtAssociation_LinkedByChangeId",
                schema: "Order",
                table: "OrderItemServiceLinks",
                columns: new[] { "OrderIdAtAssociation", "LinkedByChangeId" },
                principalSchema: "Order",
                principalTable: "OrderChanges",
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
                name: "FK_Orders_OrderPreparations_OwnerAirlineId_SourcePreparationId",
                schema: "Order",
                table: "Orders",
                columns: new[] { "OwnerAirlineId", "SourcePreparationId" },
                principalSchema: "Order",
                principalTable: "OrderPreparations",
                principalColumns: new[] { "OwnerAirlineId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderSegments_OrderJourneys_OrderId_JourneyId",
                schema: "Order",
                table: "OrderSegments",
                columns: new[] { "OrderId", "JourneyId" },
                principalSchema: "Order",
                principalTable: "OrderJourneys",
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
                name: "FK_OrderServices_OrderItems_OrderId_OrderItemId",
                schema: "Order",
                table: "OrderServices",
                columns: new[] { "OrderId", "OrderItemId" },
                principalSchema: "Order",
                principalTable: "OrderItems",
                principalColumns: new[] { "OrderId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderTravellers_OrderTravellers_OrderId_InfantParentTravellerId",
                schema: "Order",
                table: "OrderTravellers",
                columns: new[] { "OrderId", "InfantParentTravellerId" },
                principalSchema: "Order",
                principalTable: "OrderTravellers",
                principalColumns: new[] { "OrderId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PriceChangeSets_OrderChanges_OrderId_ChangeId",
                schema: "Order",
                table: "PriceChangeSets",
                columns: new[] { "OrderId", "ChangeId" },
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

            migrationBuilder.AddForeignKey(
                name: "FK_PricingLines_PriceChangeSets_OrderId_PriceChangeSetId",
                schema: "Order",
                table: "PricingLines",
                columns: new[] { "OrderId", "PriceChangeSetId" },
                principalSchema: "Order",
                principalTable: "PriceChangeSets",
                principalColumns: new[] { "OrderId", "Id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommandReceipts_Orders_OwnerAirlineId_OrderId",
                schema: "Operations",
                table: "CommandReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_FareConstructions_OrderChanges_OrderId_CreatedByChangeId",
                schema: "Order",
                table: "FareConstructions");

            migrationBuilder.DropForeignKey(
                name: "FK_FundingObligations_OrderChanges_OrderId_ChangeId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropForeignKey(
                name: "FK_FundingObligations_OrderItems_OrderId_OrderItemId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropForeignKey(
                name: "FK_FundingObligations_OrderServices_OrderId_OrderServiceId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropForeignKey(
                name: "FK_FundingObligations_PriceChangeSets_OrderId_PriceChangeSetId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropForeignKey(
                name: "FK_FundingObligations_PricingLines_OrderId_PricingLineId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_OrderChanges_OrderId_CreatedByChangeId",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemServiceLinks_OrderChanges_OrderIdAtAssociation_LinkedByChangeId",
                schema: "Order",
                table: "OrderItemServiceLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemServiceLinks_OrderItems_OrderIdAtAssociation_OrderItemId",
                schema: "Order",
                table: "OrderItemServiceLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemServiceLinks_OrderServices_OrderIdAtAssociation_OrderServiceId",
                schema: "Order",
                table: "OrderItemServiceLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OrderPreparations_OwnerAirlineId_SourcePreparationId",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderSegments_OrderJourneys_OrderId_JourneyId",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderServices_OrderChanges_OrderId_CreatedByChangeId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderServices_OrderItems_OrderId_OrderItemId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderTravellers_OrderTravellers_OrderId_InfantParentTravellerId",
                schema: "Order",
                table: "OrderTravellers");

            migrationBuilder.DropForeignKey(
                name: "FK_PriceChangeSets_OrderChanges_OrderId_ChangeId",
                schema: "Order",
                table: "PriceChangeSets");

            migrationBuilder.DropForeignKey(
                name: "FK_PricingLines_OrderItems_OrderId_OrderItemId",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropForeignKey(
                name: "FK_PricingLines_PriceChangeSets_OrderId_PriceChangeSetId",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_PricingLines_OrderId_Id",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropIndex(
                name: "IX_PricingLines_OrderId_PriceChangeSetId",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PricingLines_BasisType_Enum",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PricingLines_CalculationKind_Enum",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PricingLines_Component_Enum",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PricingLines_Direction_Enum",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PricingLines_Effect_Enum",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PricingLines_Role_Enum",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_PriceChangeSets_OrderId_Id",
                schema: "Order",
                table: "PriceChangeSets");

            migrationBuilder.DropIndex(
                name: "IX_PriceChangeSets_OrderId_ChangeId",
                schema: "Order",
                table: "PriceChangeSets");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PriceChangeSets_Reason_Enum",
                schema: "Order",
                table: "PriceChangeSets");

            migrationBuilder.DropIndex(
                name: "IX_OrderTravellers_OrderId_InfantParentTravellerId",
                schema: "Order",
                table: "OrderTravellers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderTravellers_PassengerTypeCode_Enum",
                schema: "Order",
                table: "OrderTravellers");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_OrderServices_OrderId_Id",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropIndex(
                name: "IX_OrderServices_OrderId_CreatedByChangeId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropIndex(
                name: "IX_OrderServices_OrderId_OrderItemId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderServices_CabinBaggageWeightUnit_Enum",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderServices_CheckedBaggageWeightUnit_Enum",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderServices_CommercialStatus_Enum",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderServices_DocumentAuthority_Enum",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderServices_FulfillmentDocumentKind_Enum",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderServices_FulfillmentProfileAssurance_Enum",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderServices_FundingRequirement_Enum",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderServices_ReservationRequirement_Enum",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderServices_ServiceType_Enum",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropIndex(
                name: "IX_OrderSegments_OrderId_JourneyId",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderSegments_Kind_Enum",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Orders_OwnerAirlineId_Id",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_BuyerContextType_Enum",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_Channel_Enum",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_CommercialSummary_Enum",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_InitiatingActorContextType_Enum",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_JourneyType_Enum",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_SellerContextType_Enum",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_SellingOfficeKind_Enum",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_OrderPreparations_OwnerAirlineId_Id",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderPreparations_AcceptanceAssurance_Enum",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_OrderJourneys_OrderId_Id",
                schema: "Order",
                table: "OrderJourneys");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderJourneys_Direction_Enum",
                schema: "Order",
                table: "OrderJourneys");

            migrationBuilder.DropIndex(
                name: "IX_OrderItemServiceLinks_OrderIdAtAssociation_LinkedByChangeId",
                schema: "Order",
                table: "OrderItemServiceLinks");

            migrationBuilder.DropIndex(
                name: "IX_OrderItemServiceLinks_OrderIdAtAssociation_OrderItemId",
                schema: "Order",
                table: "OrderItemServiceLinks");

            migrationBuilder.DropIndex(
                name: "IX_OrderItemServiceLinks_OrderIdAtAssociation_OrderServiceId",
                schema: "Order",
                table: "OrderItemServiceLinks");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_OrderItems_OrderId_Id",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_OrderId_CreatedByChangeId",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_CommercialStatus_Enum",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_Kind_Enum",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderContacts_Role_Enum",
                schema: "Order",
                table: "OrderContacts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderComponentTotals_Component_Enum",
                schema: "Order",
                table: "OrderComponentTotals");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderComponentTotals_Effect_Enum",
                schema: "Order",
                table: "OrderComponentTotals");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_OrderChanges_OrderId_Id",
                schema: "Order",
                table: "OrderChanges");

            migrationBuilder.DropIndex(
                name: "IX_OrderChanges_OrderId_CommercialVersion",
                schema: "Order",
                table: "OrderChanges");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderChanges_ActorContextType_Enum",
                schema: "Order",
                table: "OrderChanges");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderChanges_Type_Enum",
                schema: "Order",
                table: "OrderChanges");

            migrationBuilder.DropIndex(
                name: "IX_FundingObligations_OrderId_ChangeId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropIndex(
                name: "IX_FundingObligations_OrderId_OrderItemId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropIndex(
                name: "IX_FundingObligations_OrderId_OrderServiceId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropIndex(
                name: "IX_FundingObligations_OrderId_PriceChangeSetId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropIndex(
                name: "IX_FundingObligations_OrderId_PricingLineId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FundingObligations_Purpose_Enum",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FarePricingUnits_SourceConstructionType_Enum",
                schema: "Order",
                table: "FarePricingUnits");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FarePricingUnits_Type_Enum",
                schema: "Order",
                table: "FarePricingUnits");

            migrationBuilder.DropIndex(
                name: "IX_FareConstructions_OrderId_CreatedByChangeId",
                schema: "Order",
                table: "FareConstructions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FareConstructions_Assurance_Enum",
                schema: "Order",
                table: "FareConstructions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CommandReceipts_CommandKind_Enum",
                schema: "Operations",
                table: "CommandReceipts");

            migrationBuilder.AlterColumn<int>(
                name: "OriginAirportId",
                schema: "Order",
                table: "OrderSegmentLegs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DestinationAirportId",
                schema: "Order",
                table: "OrderSegmentLegs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DepartureDateTime",
                schema: "Order",
                table: "OrderSegmentLegs",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ArrivalDateTime",
                schema: "Order",
                table: "OrderSegmentLegs",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.CreateIndex(
                name: "IX_PricingLines_OrderItemId",
                schema: "Order",
                table: "PricingLines",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTravellers_InfantParentTravellerId",
                schema: "Order",
                table: "OrderTravellers",
                column: "InfantParentTravellerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderServices_CreatedByChangeId",
                schema: "Order",
                table: "OrderServices",
                column: "CreatedByChangeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderServices_OrderId",
                schema: "Order",
                table: "OrderServices",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SourcePreparationId",
                schema: "Order",
                table: "Orders",
                column: "SourcePreparationId");

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
                name: "IX_OrderItemServiceLinks_OrderServiceId",
                schema: "Order",
                table: "OrderItemServiceLinks",
                column: "OrderServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_CreatedByChangeId",
                schema: "Order",
                table: "OrderItems",
                column: "CreatedByChangeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderChanges_OrderId_CommercialVersion",
                schema: "Order",
                table: "OrderChanges",
                columns: new[] { "OrderId", "CommercialVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_FundingObligations_ChangeId",
                schema: "Order",
                table: "FundingObligations",
                column: "ChangeId");

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
                name: "IX_FundingObligations_PriceChangeSetId",
                schema: "Order",
                table: "FundingObligations",
                column: "PriceChangeSetId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingObligations_PricingLineId",
                schema: "Order",
                table: "FundingObligations",
                column: "PricingLineId");

            migrationBuilder.CreateIndex(
                name: "IX_FareConstructions_CreatedByChangeId",
                schema: "Order",
                table: "FareConstructions",
                column: "CreatedByChangeId");

            migrationBuilder.AddForeignKey(
                name: "FK_FareConstructions_OrderChanges_CreatedByChangeId",
                schema: "Order",
                table: "FareConstructions",
                column: "CreatedByChangeId",
                principalSchema: "Order",
                principalTable: "OrderChanges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FundingObligations_OrderChanges_ChangeId",
                schema: "Order",
                table: "FundingObligations",
                column: "ChangeId",
                principalSchema: "Order",
                principalTable: "OrderChanges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_OrderChanges_CreatedByChangeId",
                schema: "Order",
                table: "OrderItems",
                column: "CreatedByChangeId",
                principalSchema: "Order",
                principalTable: "OrderChanges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemServiceLinks_OrderChanges_LinkedByChangeId",
                schema: "Order",
                table: "OrderItemServiceLinks",
                column: "LinkedByChangeId",
                principalSchema: "Order",
                principalTable: "OrderChanges",
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
                name: "FK_Orders_OrderPreparations_SourcePreparationId",
                schema: "Order",
                table: "Orders",
                column: "SourcePreparationId",
                principalSchema: "Order",
                principalTable: "OrderPreparations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderSegments_OrderJourneys_JourneyId",
                schema: "Order",
                table: "OrderSegments",
                column: "JourneyId",
                principalSchema: "Order",
                principalTable: "OrderJourneys",
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
                name: "FK_OrderServices_OrderItems_OrderItemId",
                schema: "Order",
                table: "OrderServices",
                column: "OrderItemId",
                principalSchema: "Order",
                principalTable: "OrderItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderTravellers_OrderTravellers_InfantParentTravellerId",
                schema: "Order",
                table: "OrderTravellers",
                column: "InfantParentTravellerId",
                principalSchema: "Order",
                principalTable: "OrderTravellers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PriceChangeSets_OrderChanges_ChangeId",
                schema: "Order",
                table: "PriceChangeSets",
                column: "ChangeId",
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

            migrationBuilder.AddForeignKey(
                name: "FK_PricingLines_PriceChangeSets_PriceChangeSetId",
                schema: "Order",
                table: "PricingLines",
                column: "PriceChangeSetId",
                principalSchema: "Order",
                principalTable: "PriceChangeSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
