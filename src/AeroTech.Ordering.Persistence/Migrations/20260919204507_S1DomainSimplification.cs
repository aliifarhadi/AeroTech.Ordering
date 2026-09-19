using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ordering.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class S1DomainSimplification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FareConstructions_FareConstructions_SupersededByConstructionId",
                schema: "Order",
                table: "FareConstructions");

            migrationBuilder.DropForeignKey(
                name: "FK_FareConstructions_Orders_OrderIdAtCreation",
                schema: "Order",
                table: "FareConstructions");

            migrationBuilder.DropForeignKey(
                name: "FK_FarePricingUnits_FarePricingGroups_PricingGroupId",
                schema: "Order",
                table: "FarePricingUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_FundingObligations_FundingObligations_SupersededObligationId",
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
                name: "FK_PricingLines_PricingLines_OriginalPricingLineId",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropTable(
                name: "AirTransportServiceDetails",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "FareComponentSegments",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "FareComponentServices",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "FarePricingGroupTravelers",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "OrderComponentTotals",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "OrderItemServiceLinkSegments",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "OrderItemServiceLinkTravelers",
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
                name: "FarePricingGroups",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "OrderItemServiceLinks",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "OrderTravelers",
                schema: "Order");

            migrationBuilder.DropIndex(
                name: "IX_PricingLines_OriginalPricingLineId",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropIndex(
                name: "IX_PricingLines_PriceChangeSetId_CandidateLineRef",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PricingLines_ReversalReference",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropIndex(
                name: "IX_PriceChangeSets_ChangeId",
                schema: "Order",
                table: "PriceChangeSets");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PriceChangeSets_Sequence",
                schema: "Order",
                table: "PriceChangeSets");

            migrationBuilder.DropIndex(
                name: "IX_OrderServices_OrderId_SourceServiceRef",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderServices_CapacityUnits",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderServices_Quantity",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderServices_Version",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropIndex(
                name: "IX_OrderSegments_JourneyId",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropIndex(
                name: "IX_OrderSegments_OrderId_Sequence",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropIndex(
                name: "IX_OrderSegments_OrderId_SourceSegmentRef",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderSegments_Duration",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropIndex(
                name: "IX_OrderPreparations_ConsumedAt_CreatedAt",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropIndex(
                name: "IX_OrderPreparations_OwnerAirlineId_FinancialCustomerId_CreatedAt",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropIndex(
                name: "UX_OrderPreparations_ConsumedByOrderId",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderPreparations_Consumption",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_OrderId_SourceItemRef",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderChanges_OrderId_CommercialVersion",
                schema: "Order",
                table: "OrderChanges");

            migrationBuilder.DropIndex(
                name: "IX_FundingObligations_OrderServiceId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropIndex(
                name: "IX_FundingObligations_PricingLineId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropIndex(
                name: "IX_FundingObligations_SupersededObligationId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FundingObligations_ExactlyOneScope",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropIndex(
                name: "IX_FarePricingUnits_FareConstructionId_SourceUnitRef",
                schema: "Order",
                table: "FarePricingUnits");

            migrationBuilder.DropIndex(
                name: "IX_FarePricingUnits_PricingGroupId",
                schema: "Order",
                table: "FarePricingUnits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FarePricingUnitCoveredBounds",
                schema: "Order",
                table: "FarePricingUnitCoveredBounds");

            migrationBuilder.DropIndex(
                name: "IX_FarePricingUnitCoveredBounds_PricingUnitId_SourceBoundRef",
                schema: "Order",
                table: "FarePricingUnitCoveredBounds");

            migrationBuilder.DropIndex(
                name: "IX_FareConstructions_SupersededByConstructionId",
                schema: "Order",
                table: "FareConstructions");

            migrationBuilder.DropIndex(
                name: "UX_FareConstructions_CurrentPerOrder",
                schema: "Order",
                table: "FareConstructions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FareConstructionItems",
                schema: "Order",
                table: "FareConstructionItems");

            migrationBuilder.DropIndex(
                name: "IX_FareConstructionItems_FareConstructionId_OrderItemId",
                schema: "Order",
                table: "FareConstructionItems");

            migrationBuilder.DropIndex(
                name: "IX_CommandReceipts_OperationId",
                schema: "Operations",
                table: "CommandReceipts");

            migrationBuilder.DropColumn(
                name: "CandidateLineRef",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "ConversionFromCurrencyRef",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "ConversionToCurrencyRef",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "OriginalPricingLineId",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "OriginalValueCurrencyRef",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "SaleValueCurrencyRef",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "SourceBasisRef",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "BaseCommercialVersion",
                schema: "Order",
                table: "PriceChangeSets");

            migrationBuilder.DropColumn(
                name: "SourceDecisionRef",
                schema: "Order",
                table: "PriceChangeSets");

            migrationBuilder.DropColumn(
                name: "DeliveryControlPolicyRef",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "DeliveryProviderRef",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "DependencyTreatmentPolicyRef",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "DetailSchema",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "DetailSchemaVersion",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "DocumentKind",
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
                name: "Name",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "PartialFulfillmentSupported",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "PriceTreatment",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "Quantity",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "QuantityUnit",
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
                name: "ServiceCode",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "ServiceVersion",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "SourceServiceRef",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "Type",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "AircraftRef",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropColumn(
                name: "DestinationRef",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropColumn(
                name: "DestinationTerminalRef",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropColumn(
                name: "FlightRef",
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
                name: "OriginRef",
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
                name: "SourceSegmentRef",
                schema: "Order",
                table: "OrderSegments");

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
                name: "SourceLegRef",
                schema: "Order",
                table: "OrderSegmentLegs");

            migrationBuilder.DropColumn(
                name: "AcceptanceAssurance",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AcceptanceProfile",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AcceptedContractVersion",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AcceptedOwnerBindingRef",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AcceptedPreparationId",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AcceptedProviderProfileId",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AcceptedSourceOwner",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AcceptedSourcePayloadHash",
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

            migrationBuilder.DropColumn(
                name: "ClientAcceptedAt",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CustomerTotalCurrencyRef",
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
                name: "OfferValidityOwner",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OfferValidityReason",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OfferValiditySourceRef",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OfferValidityState",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OfferValidityValue",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PriceValidityOwner",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PriceValidityReason",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PriceValiditySourceRef",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PriceValidityValue",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SaleCurrencyCode",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SaleCurrencyRef",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SourceCapturedAt",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SourceJourneyTypeRaw",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SourcePricedAt",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TicketingValidityOwner",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TicketingValidityReason",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TicketingValiditySourceRef",
                schema: "Order",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ActorContextType",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropColumn(
                name: "ActorId",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropColumn(
                name: "Channel",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropColumn(
                name: "ClientReference",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropColumn(
                name: "ConsumedAt",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropColumn(
                name: "ConsumedByOrderId",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropColumn(
                name: "OfferValidityOwner",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropColumn(
                name: "OfferValidityReason",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropColumn(
                name: "OfferValiditySourceRef",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropColumn(
                name: "OfferValidityState",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropColumn(
                name: "OfferValidityValue",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropColumn(
                name: "PriceValidityOwner",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropColumn(
                name: "PriceValidityReason",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropColumn(
                name: "PriceValiditySourceRef",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropColumn(
                name: "PriceValidityState",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropColumn(
                name: "SellingOfficeId",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropColumn(
                name: "TicketingValidityOwner",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropColumn(
                name: "TicketingValidityReason",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropColumn(
                name: "TicketingValiditySourceRef",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropColumn(
                name: "TicketingValidityState",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropColumn(
                name: "DestinationRef",
                schema: "Order",
                table: "OrderJourneys");

            migrationBuilder.DropColumn(
                name: "OriginRef",
                schema: "Order",
                table: "OrderJourneys");

            migrationBuilder.DropColumn(
                name: "SourceDirectionRaw",
                schema: "Order",
                table: "OrderJourneys");

            migrationBuilder.DropColumn(
                name: "AcceptedTotalCurrencyRef",
                schema: "Order",
                table: "OrderItems");

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
                name: "SourceItemRef",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "SourceOfferItemRef",
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
                name: "SourceDecisionRef",
                schema: "Order",
                table: "OrderChanges");

            migrationBuilder.DropColumn(
                name: "AmountCurrencyRef",
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
                name: "SourceDecisionRef",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropColumn(
                name: "SupersededObligationId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropColumn(
                name: "CombinationMethod",
                schema: "Order",
                table: "FarePricingUnits");

            migrationBuilder.DropColumn(
                name: "PricingGroupId",
                schema: "Order",
                table: "FarePricingUnits");

            migrationBuilder.DropColumn(
                name: "SourceKindRaw",
                schema: "Order",
                table: "FarePricingUnits");

            migrationBuilder.DropColumn(
                name: "SourceUnitRef",
                schema: "Order",
                table: "FarePricingUnits");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "Order",
                table: "FarePricingUnitCoveredBounds");

            migrationBuilder.DropColumn(
                name: "LastUpdateTime",
                schema: "Order",
                table: "FarePricingUnitCoveredBounds");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                schema: "Order",
                table: "FarePricingUnitCoveredBounds");

            migrationBuilder.DropColumn(
                name: "Assurance",
                schema: "Order",
                table: "FareConstructions");

            migrationBuilder.DropColumn(
                name: "SourceContextRef",
                schema: "Order",
                table: "FareConstructions");

            migrationBuilder.DropColumn(
                name: "SupersededByConstructionId",
                schema: "Order",
                table: "FareConstructions");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "Order",
                table: "FareConstructionItems");

            migrationBuilder.DropColumn(
                name: "LastUpdateTime",
                schema: "Order",
                table: "FareConstructionItems");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                schema: "Order",
                table: "FareConstructionItems");

            migrationBuilder.DropColumn(
                name: "CabinRef",
                schema: "Order",
                table: "FareComponents");

            migrationBuilder.DropColumn(
                name: "FareOwnerRef",
                schema: "Order",
                table: "FareComponents");

            migrationBuilder.DropColumn(
                name: "RbdRef",
                schema: "Order",
                table: "FareComponents");

            migrationBuilder.DropColumn(
                name: "RoutingRef",
                schema: "Order",
                table: "FareComponents");

            migrationBuilder.DropColumn(
                name: "RuleRef",
                schema: "Order",
                table: "FareComponents");

            migrationBuilder.DropColumn(
                name: "SourceFareRef",
                schema: "Order",
                table: "FareComponents");

            migrationBuilder.DropColumn(
                name: "TariffRef",
                schema: "Order",
                table: "FareComponents");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                schema: "Operations",
                table: "CommandReceipts");

            migrationBuilder.DropColumn(
                name: "OperationId",
                schema: "Operations",
                table: "CommandReceipts");

            migrationBuilder.DropColumn(
                name: "PreparationId",
                schema: "Operations",
                table: "CommandReceipts");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "Operations",
                table: "CommandReceipts");

            migrationBuilder.RenameColumn(
                name: "SourceReference",
                schema: "Order",
                table: "PricingLines",
                newName: "Reference");

            migrationBuilder.RenameColumn(
                name: "SourceName",
                schema: "Order",
                table: "PricingLines",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "SourceLineRef",
                schema: "Order",
                table: "PricingLines",
                newName: "SourceOccurrencePath");

            migrationBuilder.RenameColumn(
                name: "SourceCode",
                schema: "Order",
                table: "PricingLines",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "Role",
                schema: "Order",
                table: "PricingLines",
                newName: "SaleValueCurrencyId");

            migrationBuilder.RenameColumn(
                name: "SupplierPartyRef",
                schema: "Order",
                table: "OrderServices",
                newName: "BookingClass");

            migrationBuilder.RenameColumn(
                name: "DocumentAuthority",
                schema: "Order",
                table: "OrderServices",
                newName: "CheckedBaggageWeightUnit");

            migrationBuilder.RenameColumn(
                name: "CapacityUnits",
                schema: "Order",
                table: "OrderServices",
                newName: "CheckedBaggagePieces");

            migrationBuilder.RenameColumn(
                name: "Departure",
                schema: "Order",
                table: "OrderSegmentLegs",
                newName: "DepartureDateTime");

            migrationBuilder.RenameColumn(
                name: "Arrival",
                schema: "Order",
                table: "OrderSegmentLegs",
                newName: "ArrivalDateTime");

            migrationBuilder.RenameColumn(
                name: "AcceptedSourceOfferId",
                schema: "Order",
                table: "Orders",
                newName: "SourceOfferId");

            migrationBuilder.RenameColumn(
                name: "TicketingValidityValue",
                schema: "Order",
                table: "Orders",
                newName: "LastTicketingDate");

            migrationBuilder.RenameColumn(
                name: "TicketingValidityState",
                schema: "Order",
                table: "Orders",
                newName: "CustomerTotalCurrencyId");

            migrationBuilder.RenameColumn(
                name: "PriceValidityState",
                schema: "Order",
                table: "Orders",
                newName: "CurrencyId");

            migrationBuilder.RenameColumn(
                name: "TicketingValidityValue",
                schema: "Order",
                table: "OrderPreparations",
                newName: "PriceValidUntil");

            migrationBuilder.RenameColumn(
                name: "PriceValidityValue",
                schema: "Order",
                table: "OrderPreparations",
                newName: "OfferExpiresAt");

            migrationBuilder.RenameColumn(
                name: "SourceBoundRef",
                schema: "Order",
                table: "OrderJourneys",
                newName: "BoundId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderJourneys_OrderId_SourceBoundRef",
                schema: "Order",
                table: "OrderJourneys",
                newName: "IX_OrderJourneys_OrderId_BoundId");

            migrationBuilder.RenameColumn(
                name: "TermsUpgradeEligibility",
                schema: "Order",
                table: "OrderItems",
                newName: "AcceptedTotalCurrencyId");

            migrationBuilder.RenameColumn(
                name: "SourceBoundRef",
                schema: "Order",
                table: "FarePricingUnitCoveredBounds",
                newName: "CoveredBoundOfferId");

            migrationBuilder.RenameColumn(
                name: "OrderIdAtCreation",
                schema: "Order",
                table: "FareConstructions",
                newName: "OrderId");

            migrationBuilder.AddColumn<int>(
                name: "ConversionFromCurrencyId",
                schema: "Order",
                table: "PricingLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConversionToCurrencyId",
                schema: "Order",
                table: "PricingLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginalValueCurrencyId",
                schema: "Order",
                table: "PricingLines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CabinBaggagePieces",
                schema: "Order",
                table: "OrderServices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CabinBaggageWeight",
                schema: "Order",
                table: "OrderServices",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CabinBaggageWeightUnit",
                schema: "Order",
                table: "OrderServices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CabinClassId",
                schema: "Order",
                table: "OrderServices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CheckedBaggageWeight",
                schema: "Order",
                table: "OrderServices",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RbdId",
                schema: "Order",
                table: "OrderServices",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SegmentId",
                schema: "Order",
                table: "OrderServices",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "TravellerId",
                schema: "Order",
                table: "OrderServices",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<long>(
                name: "JourneyId",
                schema: "Order",
                table: "OrderSegments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FlightVersion",
                schema: "Order",
                table: "OrderSegments",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AircraftId",
                schema: "Order",
                table: "OrderSegments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DestinationAirportId",
                schema: "Order",
                table: "OrderSegments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DestinationAirportTerminalId",
                schema: "Order",
                table: "OrderSegments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FlightCapacityId",
                schema: "Order",
                table: "OrderSegments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FlightId",
                schema: "Order",
                table: "OrderSegments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MarketingAirlineId",
                schema: "Order",
                table: "OrderSegments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OperatingAirlineId",
                schema: "Order",
                table: "OrderSegments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginAirportId",
                schema: "Order",
                table: "OrderSegments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OriginAirportTerminalId",
                schema: "Order",
                table: "OrderSegments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DestinationAirportId",
                schema: "Order",
                table: "OrderSegmentLegs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DestinationAirportTerminalId",
                schema: "Order",
                table: "OrderSegmentLegs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LegId",
                schema: "Order",
                table: "OrderSegmentLegs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "OriginAirportId",
                schema: "Order",
                table: "OrderSegmentLegs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginAirportTerminalId",
                schema: "Order",
                table: "OrderSegmentLegs",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "JourneyType",
                schema: "Order",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Direction",
                schema: "Order",
                table: "OrderJourneys",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DestinationAirportId",
                schema: "Order",
                table: "OrderJourneys",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OriginAirportId",
                schema: "Order",
                table: "OrderJourneys",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.AddColumn<int>(
                name: "AmountCurrencyId",
                schema: "Order",
                table: "FundingObligations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "AirFareId",
                schema: "Order",
                table: "FareComponents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "CabinClassId",
                schema: "Order",
                table: "FareComponents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RbdId",
                schema: "Order",
                table: "FareComponents",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FarePricingUnitCoveredBounds",
                schema: "Order",
                table: "FarePricingUnitCoveredBounds",
                columns: new[] { "PricingUnitId", "CoveredBoundOfferId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_FareConstructionItems",
                schema: "Order",
                table: "FareConstructionItems",
                columns: new[] { "FareConstructionId", "OrderItemId" });

            migrationBuilder.CreateTable(
                name: "OrderTravellers",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    SourceTravellerRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ClientTravellerRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PassengerTypeCode = table.Column<int>(type: "int", nullable: false),
                    InfantParentTravellerId = table.Column<long>(type: "bigint", nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTravellers", x => x.Id);
                    table.CheckConstraint("CK_OrderTravellers_Guardian", "[InfantParentTravellerId] IS NULL OR [InfantParentTravellerId] <> [Id]");
                    table.ForeignKey(
                        name: "FK_OrderTravellers_OrderTravellers_InfantParentTravellerId",
                        column: x => x.InfantParentTravellerId,
                        principalSchema: "Order",
                        principalTable: "OrderTravellers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderTravellers_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "Order",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderTravellerIdentities",
                schema: "Order",
                columns: table => new
                {
                    TravellerId = table.Column<long>(type: "bigint", nullable: false),
                    GivenName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTravellerIdentities", x => x.TravellerId);
                    table.ForeignKey(
                        name: "FK_OrderTravellerIdentities_OrderTravellers_TravellerId",
                        column: x => x.TravellerId,
                        principalSchema: "Order",
                        principalTable: "OrderTravellers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PricingLines_PriceChangeSetId_SourceOccurrencePath",
                schema: "Order",
                table: "PricingLines",
                columns: new[] { "PriceChangeSetId", "SourceOccurrencePath" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceChangeSets_ChangeId",
                schema: "Order",
                table: "PriceChangeSets",
                column: "ChangeId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PriceChangeSets_FinancialSequence",
                schema: "Order",
                table: "PriceChangeSets",
                sql: "[FinancialSequence] >= 1");

            migrationBuilder.CreateIndex(
                name: "IX_OrderServices_OrderId_TravellerId_SegmentId",
                schema: "Order",
                table: "OrderServices",
                columns: new[] { "OrderId", "TravellerId", "SegmentId" },
                unique: true);

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
                name: "IX_OrderSegments_JourneyId_Sequence",
                schema: "Order",
                table: "OrderSegments",
                columns: new[] { "JourneyId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderSegments_OrderId",
                schema: "Order",
                table: "OrderSegments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderSegmentLegs_SegmentId_LegId",
                schema: "Order",
                table: "OrderSegmentLegs",
                columns: new[] { "SegmentId", "LegId" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderSegmentLegs_Sequence",
                schema: "Order",
                table: "OrderSegmentLegs",
                sql: "[Sequence] >= 1");

            migrationBuilder.CreateIndex(
                name: "IX_OrderPreparations_OwnerAirlineId_FinancialCustomerId_CapturedAt",
                schema: "Order",
                table: "OrderPreparations",
                columns: new[] { "OwnerAirlineId", "FinancialCustomerId", "CapturedAt" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_AcceptedTotal",
                schema: "Order",
                table: "OrderItems",
                sql: "[AcceptedTotalAmount] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_OrderChanges_OrderId_CommercialVersion",
                schema: "Order",
                table: "OrderChanges",
                columns: new[] { "OrderId", "CommercialVersion" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderChanges_CommercialVersion",
                schema: "Order",
                table: "OrderChanges",
                sql: "[CommercialVersion] >= 1");

            migrationBuilder.CreateIndex(
                name: "IX_FarePricingUnits_FareConstructionId_Sequence",
                schema: "Order",
                table: "FarePricingUnits",
                columns: new[] { "FareConstructionId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FareConstructions_OrderId",
                schema: "Order",
                table: "FareConstructions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTravellers_InfantParentTravellerId",
                schema: "Order",
                table: "OrderTravellers",
                column: "InfantParentTravellerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTravellers_OrderId_ClientTravellerRef",
                schema: "Order",
                table: "OrderTravellers",
                columns: new[] { "OrderId", "ClientTravellerRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderTravellers_OrderId_SourceTravellerRef",
                schema: "Order",
                table: "OrderTravellers",
                columns: new[] { "OrderId", "SourceTravellerRef" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FareConstructions_Orders_OrderId",
                schema: "Order",
                table: "FareConstructions",
                column: "OrderId",
                principalSchema: "Order",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FareConstructions_Orders_OrderId",
                schema: "Order",
                table: "FareConstructions");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderServices_OrderSegments_SegmentId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderServices_OrderTravellers_TravellerId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropTable(
                name: "OrderTravellerIdentities",
                schema: "Order");

            migrationBuilder.DropTable(
                name: "OrderTravellers",
                schema: "Order");

            migrationBuilder.DropIndex(
                name: "IX_PricingLines_PriceChangeSetId_SourceOccurrencePath",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropIndex(
                name: "IX_PriceChangeSets_ChangeId",
                schema: "Order",
                table: "PriceChangeSets");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PriceChangeSets_FinancialSequence",
                schema: "Order",
                table: "PriceChangeSets");

            migrationBuilder.DropIndex(
                name: "IX_OrderServices_OrderId_TravellerId_SegmentId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropIndex(
                name: "IX_OrderServices_SegmentId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropIndex(
                name: "IX_OrderServices_TravellerId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropIndex(
                name: "IX_OrderSegments_JourneyId_Sequence",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropIndex(
                name: "IX_OrderSegments_OrderId",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropIndex(
                name: "IX_OrderSegmentLegs_SegmentId_LegId",
                schema: "Order",
                table: "OrderSegmentLegs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderSegmentLegs_Sequence",
                schema: "Order",
                table: "OrderSegmentLegs");

            migrationBuilder.DropIndex(
                name: "IX_OrderPreparations_OwnerAirlineId_FinancialCustomerId_CapturedAt",
                schema: "Order",
                table: "OrderPreparations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_AcceptedTotal",
                schema: "Order",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderChanges_OrderId_CommercialVersion",
                schema: "Order",
                table: "OrderChanges");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderChanges_CommercialVersion",
                schema: "Order",
                table: "OrderChanges");

            migrationBuilder.DropIndex(
                name: "IX_FarePricingUnits_FareConstructionId_Sequence",
                schema: "Order",
                table: "FarePricingUnits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FarePricingUnitCoveredBounds",
                schema: "Order",
                table: "FarePricingUnitCoveredBounds");

            migrationBuilder.DropIndex(
                name: "IX_FareConstructions_OrderId",
                schema: "Order",
                table: "FareConstructions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FareConstructionItems",
                schema: "Order",
                table: "FareConstructionItems");

            migrationBuilder.DropColumn(
                name: "ConversionFromCurrencyId",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "ConversionToCurrencyId",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "OriginalValueCurrencyId",
                schema: "Order",
                table: "PricingLines");

            migrationBuilder.DropColumn(
                name: "CabinBaggagePieces",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "CabinBaggageWeight",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "CabinBaggageWeightUnit",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "CabinClassId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "CheckedBaggageWeight",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "RbdId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "SegmentId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "TravellerId",
                schema: "Order",
                table: "OrderServices");

            migrationBuilder.DropColumn(
                name: "AircraftId",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropColumn(
                name: "DestinationAirportId",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropColumn(
                name: "DestinationAirportTerminalId",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropColumn(
                name: "FlightCapacityId",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropColumn(
                name: "FlightId",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropColumn(
                name: "MarketingAirlineId",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropColumn(
                name: "OperatingAirlineId",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropColumn(
                name: "OriginAirportId",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropColumn(
                name: "OriginAirportTerminalId",
                schema: "Order",
                table: "OrderSegments");

            migrationBuilder.DropColumn(
                name: "DestinationAirportId",
                schema: "Order",
                table: "OrderSegmentLegs");

            migrationBuilder.DropColumn(
                name: "DestinationAirportTerminalId",
                schema: "Order",
                table: "OrderSegmentLegs");

            migrationBuilder.DropColumn(
                name: "LegId",
                schema: "Order",
                table: "OrderSegmentLegs");

            migrationBuilder.DropColumn(
                name: "OriginAirportId",
                schema: "Order",
                table: "OrderSegmentLegs");

            migrationBuilder.DropColumn(
                name: "OriginAirportTerminalId",
                schema: "Order",
                table: "OrderSegmentLegs");

            migrationBuilder.DropColumn(
                name: "DestinationAirportId",
                schema: "Order",
                table: "OrderJourneys");

            migrationBuilder.DropColumn(
                name: "OriginAirportId",
                schema: "Order",
                table: "OrderJourneys");

            migrationBuilder.DropColumn(
                name: "AmountCurrencyId",
                schema: "Order",
                table: "FundingObligations");

            migrationBuilder.DropColumn(
                name: "AirFareId",
                schema: "Order",
                table: "FareComponents");

            migrationBuilder.DropColumn(
                name: "CabinClassId",
                schema: "Order",
                table: "FareComponents");

            migrationBuilder.DropColumn(
                name: "RbdId",
                schema: "Order",
                table: "FareComponents");

            migrationBuilder.RenameColumn(
                name: "SourceOccurrencePath",
                schema: "Order",
                table: "PricingLines",
                newName: "SourceLineRef");

            migrationBuilder.RenameColumn(
                name: "SaleValueCurrencyId",
                schema: "Order",
                table: "PricingLines",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "Reference",
                schema: "Order",
                table: "PricingLines",
                newName: "SourceReference");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "Order",
                table: "PricingLines",
                newName: "SourceName");

            migrationBuilder.RenameColumn(
                name: "Code",
                schema: "Order",
                table: "PricingLines",
                newName: "SourceCode");

            migrationBuilder.RenameColumn(
                name: "CheckedBaggageWeightUnit",
                schema: "Order",
                table: "OrderServices",
                newName: "DocumentAuthority");

            migrationBuilder.RenameColumn(
                name: "CheckedBaggagePieces",
                schema: "Order",
                table: "OrderServices",
                newName: "CapacityUnits");

            migrationBuilder.RenameColumn(
                name: "BookingClass",
                schema: "Order",
                table: "OrderServices",
                newName: "SupplierPartyRef");

            migrationBuilder.RenameColumn(
                name: "DepartureDateTime",
                schema: "Order",
                table: "OrderSegmentLegs",
                newName: "Departure");

            migrationBuilder.RenameColumn(
                name: "ArrivalDateTime",
                schema: "Order",
                table: "OrderSegmentLegs",
                newName: "Arrival");

            migrationBuilder.RenameColumn(
                name: "SourceOfferId",
                schema: "Order",
                table: "Orders",
                newName: "AcceptedSourceOfferId");

            migrationBuilder.RenameColumn(
                name: "LastTicketingDate",
                schema: "Order",
                table: "Orders",
                newName: "TicketingValidityValue");

            migrationBuilder.RenameColumn(
                name: "CustomerTotalCurrencyId",
                schema: "Order",
                table: "Orders",
                newName: "TicketingValidityState");

            migrationBuilder.RenameColumn(
                name: "CurrencyId",
                schema: "Order",
                table: "Orders",
                newName: "PriceValidityState");

            migrationBuilder.RenameColumn(
                name: "PriceValidUntil",
                schema: "Order",
                table: "OrderPreparations",
                newName: "TicketingValidityValue");

            migrationBuilder.RenameColumn(
                name: "OfferExpiresAt",
                schema: "Order",
                table: "OrderPreparations",
                newName: "PriceValidityValue");

            migrationBuilder.RenameColumn(
                name: "BoundId",
                schema: "Order",
                table: "OrderJourneys",
                newName: "SourceBoundRef");

            migrationBuilder.RenameIndex(
                name: "IX_OrderJourneys_OrderId_BoundId",
                schema: "Order",
                table: "OrderJourneys",
                newName: "IX_OrderJourneys_OrderId_SourceBoundRef");

            migrationBuilder.RenameColumn(
                name: "AcceptedTotalCurrencyId",
                schema: "Order",
                table: "OrderItems",
                newName: "TermsUpgradeEligibility");

            migrationBuilder.RenameColumn(
                name: "CoveredBoundOfferId",
                schema: "Order",
                table: "FarePricingUnitCoveredBounds",
                newName: "SourceBoundRef");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                schema: "Order",
                table: "FareConstructions",
                newName: "OrderIdAtCreation");

            migrationBuilder.AddColumn<string>(
                name: "CandidateLineRef",
                schema: "Order",
                table: "PricingLines",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConversionFromCurrencyRef",
                schema: "Order",
                table: "PricingLines",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConversionToCurrencyRef",
                schema: "Order",
                table: "PricingLines",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "OriginalPricingLineId",
                schema: "Order",
                table: "PricingLines",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalValueCurrencyRef",
                schema: "Order",
                table: "PricingLines",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaleValueCurrencyRef",
                schema: "Order",
                table: "PricingLines",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourceBasisRef",
                schema: "Order",
                table: "PricingLines",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BaseCommercialVersion",
                schema: "Order",
                table: "PriceChangeSets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SourceDecisionRef",
                schema: "Order",
                table: "PriceChangeSets",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryControlPolicyRef",
                schema: "Order",
                table: "OrderServices",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryProviderRef",
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

            migrationBuilder.AddColumn<string>(
                name: "DetailSchema",
                schema: "Order",
                table: "OrderServices",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DetailSchemaVersion",
                schema: "Order",
                table: "OrderServices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DocumentKind",
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
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FulfillmentProfileVersion",
                schema: "Order",
                table: "OrderServices",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FundingRequirement",
                schema: "Order",
                table: "OrderServices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
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

            migrationBuilder.AddColumn<int>(
                name: "PriceTreatment",
                schema: "Order",
                table: "OrderServices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                schema: "Order",
                table: "OrderServices",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "QuantityUnit",
                schema: "Order",
                table: "OrderServices",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.AddColumn<string>(
                name: "ServiceCode",
                schema: "Order",
                table: "OrderServices",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServiceVersion",
                schema: "Order",
                table: "OrderServices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SourceServiceRef",
                schema: "Order",
                table: "OrderServices",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                schema: "Order",
                table: "OrderServices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<long>(
                name: "JourneyId",
                schema: "Order",
                table: "OrderSegments",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "FlightVersion",
                schema: "Order",
                table: "OrderSegments",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AircraftRef",
                schema: "Order",
                table: "OrderSegments",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationRef",
                schema: "Order",
                table: "OrderSegments",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DestinationTerminalRef",
                schema: "Order",
                table: "OrderSegments",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlightRef",
                schema: "Order",
                table: "OrderSegments",
                type: "nvarchar(128)",
                maxLength: 128,
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
                name: "OriginRef",
                schema: "Order",
                table: "OrderSegments",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AddColumn<string>(
                name: "SourceSegmentRef",
                schema: "Order",
                table: "OrderSegments",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AddColumn<string>(
                name: "SourceLegRef",
                schema: "Order",
                table: "OrderSegmentLegs",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "JourneyType",
                schema: "Order",
                table: "Orders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AcceptanceAssurance",
                schema: "Order",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AcceptanceProfile",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AcceptedAt",
                schema: "Order",
                table: "Orders",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "AcceptedContractVersion",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AcceptedOwnerBindingRef",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AcceptedPreparationId",
                schema: "Order",
                table: "Orders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "AcceptedProviderProfileId",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AcceptedSourceOwner",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AcceptedSourcePayloadHash",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ClientAcceptedAt",
                schema: "Order",
                table: "Orders",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "CustomerTotalCurrencyRef",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

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
                name: "OfferValidityOwner",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OfferValidityReason",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfferValiditySourceRef",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OfferValidityState",
                schema: "Order",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "OfferValidityValue",
                schema: "Order",
                table: "Orders",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriceValidityOwner",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PriceValidityReason",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriceValiditySourceRef",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PriceValidityValue",
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
                name: "SaleCurrencyRef",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SourceCapturedAt",
                schema: "Order",
                table: "Orders",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "SourceJourneyTypeRaw",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SourcePricedAt",
                schema: "Order",
                table: "Orders",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "TicketingValidityOwner",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TicketingValidityReason",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TicketingValiditySourceRef",
                schema: "Order",
                table: "Orders",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActorContextType",
                schema: "Order",
                table: "OrderPreparations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "ActorId",
                schema: "Order",
                table: "OrderPreparations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Channel",
                schema: "Order",
                table: "OrderPreparations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ClientReference",
                schema: "Order",
                table: "OrderPreparations",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ConsumedAt",
                schema: "Order",
                table: "OrderPreparations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ConsumedByOrderId",
                schema: "Order",
                table: "OrderPreparations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "Order",
                table: "OrderPreparations",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "OfferValidityOwner",
                schema: "Order",
                table: "OrderPreparations",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OfferValidityReason",
                schema: "Order",
                table: "OrderPreparations",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfferValiditySourceRef",
                schema: "Order",
                table: "OrderPreparations",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OfferValidityState",
                schema: "Order",
                table: "OrderPreparations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "OfferValidityValue",
                schema: "Order",
                table: "OrderPreparations",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriceValidityOwner",
                schema: "Order",
                table: "OrderPreparations",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PriceValidityReason",
                schema: "Order",
                table: "OrderPreparations",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriceValiditySourceRef",
                schema: "Order",
                table: "OrderPreparations",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PriceValidityState",
                schema: "Order",
                table: "OrderPreparations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "SellingOfficeId",
                schema: "Order",
                table: "OrderPreparations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TicketingValidityOwner",
                schema: "Order",
                table: "OrderPreparations",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TicketingValidityReason",
                schema: "Order",
                table: "OrderPreparations",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TicketingValiditySourceRef",
                schema: "Order",
                table: "OrderPreparations",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TicketingValidityState",
                schema: "Order",
                table: "OrderPreparations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Direction",
                schema: "Order",
                table: "OrderJourneys",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "DestinationRef",
                schema: "Order",
                table: "OrderJourneys",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OriginRef",
                schema: "Order",
                table: "OrderJourneys",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourceDirectionRaw",
                schema: "Order",
                table: "OrderJourneys",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcceptedTotalCurrencyRef",
                schema: "Order",
                table: "OrderItems",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

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
                nullable: false,
                defaultValue: "");

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
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductVersion",
                schema: "Order",
                table: "OrderItems",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceItemRef",
                schema: "Order",
                table: "OrderItems",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourceOfferItemRef",
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
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "TermsChangeability",
                schema: "Order",
                table: "OrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TermsRefundability",
                schema: "Order",
                table: "OrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

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
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourceDecisionRef",
                schema: "Order",
                table: "OrderChanges",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<long>(
                name: "OrderItemId",
                schema: "Order",
                table: "FundingObligations",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "AmountCurrencyRef",
                schema: "Order",
                table: "FundingObligations",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AddColumn<string>(
                name: "SourceDecisionRef",
                schema: "Order",
                table: "FundingObligations",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "SupersededObligationId",
                schema: "Order",
                table: "FundingObligations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CombinationMethod",
                schema: "Order",
                table: "FarePricingUnits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "PricingGroupId",
                schema: "Order",
                table: "FarePricingUnits",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceKindRaw",
                schema: "Order",
                table: "FarePricingUnits",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceUnitRef",
                schema: "Order",
                table: "FarePricingUnits",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "Id",
                schema: "Order",
                table: "FarePricingUnitCoveredBounds",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastUpdateTime",
                schema: "Order",
                table: "FarePricingUnitCoveredBounds",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "LastUpdatedBy",
                schema: "Order",
                table: "FarePricingUnitCoveredBounds",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Assurance",
                schema: "Order",
                table: "FareConstructions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SourceContextRef",
                schema: "Order",
                table: "FareConstructions",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "SupersededByConstructionId",
                schema: "Order",
                table: "FareConstructions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Id",
                schema: "Order",
                table: "FareConstructionItems",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastUpdateTime",
                schema: "Order",
                table: "FareConstructionItems",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "LastUpdatedBy",
                schema: "Order",
                table: "FareConstructionItems",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CabinRef",
                schema: "Order",
                table: "FareComponents",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FareOwnerRef",
                schema: "Order",
                table: "FareComponents",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RbdRef",
                schema: "Order",
                table: "FareComponents",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoutingRef",
                schema: "Order",
                table: "FareComponents",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RuleRef",
                schema: "Order",
                table: "FareComponents",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceFareRef",
                schema: "Order",
                table: "FareComponents",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TariffRef",
                schema: "Order",
                table: "FareComponents",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedAt",
                schema: "Operations",
                table: "CommandReceipts",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "OperationId",
                schema: "Operations",
                table: "CommandReceipts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "PreparationId",
                schema: "Operations",
                table: "CommandReceipts",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "Operations",
                table: "CommandReceipts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FarePricingUnitCoveredBounds",
                schema: "Order",
                table: "FarePricingUnitCoveredBounds",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FareConstructionItems",
                schema: "Order",
                table: "FareConstructionItems",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AirTransportServiceDetails",
                schema: "Order",
                columns: table => new
                {
                    ServiceId = table.Column<long>(type: "bigint", nullable: false),
                    BookingClass = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CabinRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    RbdRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CabinBaggagePieces = table.Column<int>(type: "int", nullable: true),
                    CabinBaggageWeight = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    CabinBaggageWeightUnit = table.Column<int>(type: "int", nullable: true),
                    CheckedBaggagePieces = table.Column<int>(type: "int", nullable: true),
                    CheckedBaggageWeight = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    CheckedBaggageWeightUnit = table.Column<int>(type: "int", nullable: true)
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
                name: "FareComponentSegments",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    FareComponentId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    OrderSegmentId = table.Column<long>(type: "bigint", nullable: false)
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
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    OrderServiceId = table.Column<long>(type: "bigint", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "FarePricingGroups",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    FareConstructionId = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    PassengerTypeCode = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
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
                name: "OrderComponentTotals",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Component = table.Column<int>(type: "int", nullable: false),
                    CreditAmount = table.Column<decimal>(type: "decimal(28,8)", precision: 28, scale: 8, nullable: false),
                    CurrencyRef = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DebitAmount = table.Column<decimal>(type: "decimal(28,8)", precision: 28, scale: 8, nullable: false),
                    Effect = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    OrderId = table.Column<long>(type: "bigint", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "OrderItemServiceLinks",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    LinkedByChangeId = table.Column<long>(type: "bigint", nullable: false),
                    OrderIdAtAssociation = table.Column<long>(type: "bigint", nullable: false),
                    OrderItemId = table.Column<long>(type: "bigint", nullable: false),
                    OrderServiceId = table.Column<long>(type: "bigint", nullable: false)
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
                name: "OrderServiceCoverage",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    SegmentId = table.Column<long>(type: "bigint", nullable: false),
                    ServiceId = table.Column<long>(type: "bigint", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "OrderTravelers",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ClientTravelerRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    InfantParentTravelerId = table.Column<long>(type: "bigint", nullable: true),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    PassengerTypeCode = table.Column<int>(type: "int", nullable: false),
                    SourceTravellerRef = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
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
                name: "OrderItemServiceLinkSegments",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    LinkId = table.Column<long>(type: "bigint", nullable: false),
                    SegmentId = table.Column<long>(type: "bigint", nullable: false)
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
                name: "FarePricingGroupTravelers",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    PricingGroupId = table.Column<long>(type: "bigint", nullable: false),
                    TravelerId = table.Column<long>(type: "bigint", nullable: false)
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
                name: "OrderItemServiceLinkTravelers",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    LinkId = table.Column<long>(type: "bigint", nullable: false),
                    TravelerId = table.Column<long>(type: "bigint", nullable: false)
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
                name: "OrderServiceBeneficiaries",
                schema: "Order",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    ServiceId = table.Column<long>(type: "bigint", nullable: false),
                    TravelerId = table.Column<long>(type: "bigint", nullable: false)
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
                name: "OrderTravelerIdentities",
                schema: "Order",
                columns: table => new
                {
                    TravelerId = table.Column<long>(type: "bigint", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    GivenName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
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

            migrationBuilder.AddCheckConstraint(
                name: "CK_PricingLines_ReversalReference",
                schema: "Order",
                table: "PricingLines",
                sql: "[Role] <> 2 OR [OriginalPricingLineId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PriceChangeSets_ChangeId",
                schema: "Order",
                table: "PriceChangeSets",
                column: "ChangeId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_PriceChangeSets_Sequence",
                schema: "Order",
                table: "PriceChangeSets",
                sql: "[FinancialSequence] >= 1");

            migrationBuilder.CreateIndex(
                name: "IX_OrderServices_OrderId_SourceServiceRef",
                schema: "Order",
                table: "OrderServices",
                columns: new[] { "OrderId", "SourceServiceRef" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderServices_CapacityUnits",
                schema: "Order",
                table: "OrderServices",
                sql: "[CapacityUnits] IS NULL OR [CapacityUnits] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderServices_Quantity",
                schema: "Order",
                table: "OrderServices",
                sql: "[Quantity] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderServices_Version",
                schema: "Order",
                table: "OrderServices",
                sql: "[ServiceVersion] >= 1");

            migrationBuilder.CreateIndex(
                name: "IX_OrderSegments_JourneyId",
                schema: "Order",
                table: "OrderSegments",
                column: "JourneyId");

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

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderSegments_Duration",
                schema: "Order",
                table: "OrderSegments",
                sql: "[Duration] IS NULL OR [Duration] >= 0");

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

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderPreparations_Consumption",
                schema: "Order",
                table: "OrderPreparations",
                sql: "([ConsumedByOrderId] IS NULL AND [ConsumedAt] IS NULL) OR ([ConsumedByOrderId] IS NOT NULL AND [ConsumedAt] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId_SourceItemRef",
                schema: "Order",
                table: "OrderItems",
                columns: new[] { "OrderId", "SourceItemRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderChanges_OrderId_CommercialVersion",
                schema: "Order",
                table: "OrderChanges",
                columns: new[] { "OrderId", "CommercialVersion" },
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_FundingObligations_SupersededObligationId",
                schema: "Order",
                table: "FundingObligations",
                column: "SupersededObligationId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FundingObligations_ExactlyOneScope",
                schema: "Order",
                table: "FundingObligations",
                sql: "(CASE WHEN [OrderItemId] IS NULL THEN 0 ELSE 1 END) + (CASE WHEN [OrderServiceId] IS NULL THEN 0 ELSE 1 END) + (CASE WHEN [PricingLineId] IS NULL THEN 0 ELSE 1 END) = 1");

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
                name: "IX_FarePricingUnitCoveredBounds_PricingUnitId_SourceBoundRef",
                schema: "Order",
                table: "FarePricingUnitCoveredBounds",
                columns: new[] { "PricingUnitId", "SourceBoundRef" },
                unique: true);

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
                name: "IX_FareConstructionItems_FareConstructionId_OrderItemId",
                schema: "Order",
                table: "FareConstructionItems",
                columns: new[] { "FareConstructionId", "OrderItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommandReceipts_OperationId",
                schema: "Operations",
                table: "CommandReceipts",
                column: "OperationId",
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
                name: "IX_OrderComponentTotals_OrderId_Component_Effect",
                schema: "Order",
                table: "OrderComponentTotals",
                columns: new[] { "OrderId", "Component", "Effect" },
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

            migrationBuilder.AddForeignKey(
                name: "FK_FareConstructions_FareConstructions_SupersededByConstructionId",
                schema: "Order",
                table: "FareConstructions",
                column: "SupersededByConstructionId",
                principalSchema: "Order",
                principalTable: "FareConstructions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FareConstructions_Orders_OrderIdAtCreation",
                schema: "Order",
                table: "FareConstructions",
                column: "OrderIdAtCreation",
                principalSchema: "Order",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FarePricingUnits_FarePricingGroups_PricingGroupId",
                schema: "Order",
                table: "FarePricingUnits",
                column: "PricingGroupId",
                principalSchema: "Order",
                principalTable: "FarePricingGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FundingObligations_FundingObligations_SupersededObligationId",
                schema: "Order",
                table: "FundingObligations",
                column: "SupersededObligationId",
                principalSchema: "Order",
                principalTable: "FundingObligations",
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
                name: "FK_PricingLines_PricingLines_OriginalPricingLineId",
                schema: "Order",
                table: "PricingLines",
                column: "OriginalPricingLineId",
                principalSchema: "Order",
                principalTable: "PricingLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
