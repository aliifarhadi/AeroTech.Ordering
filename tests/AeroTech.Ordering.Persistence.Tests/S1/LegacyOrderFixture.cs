namespace AeroTech.Ordering.Persistence.Tests.S1
{
    public static class LegacyOrderFixture
    {
        public const long PreparationId = 700001;
        public const long OrderId = 700002;
        public const long ChangeId = 700003;
        public const long PriceChangeSetId = 700004;
        public const long TravelerId = 700005;
        public const long SegmentId = 700006;
        public const long ItemId = 700007;
        public const long ServiceId = 700008;
        public const long LinkId = 700009;
        public const long PricingLineId = 700010;
        public const long FareConstructionId = 700011;

        public const string PricingUnitsJson =
            """[{"combinationMethod":"ProviderDefined","components":[{"bookingClass":"Y","cabinRef":"CABIN-1","coveredServiceRefs":[],"fareBasis":"YOW","fareFamily":"FLEX","fareType":"Published","rbdRef":"RBD-1","sourceFareRef":"AIRFARE-77"}],"coveredSourceBoundRefs":["BOUND-1"],"pricingGroup":null,"sourceUnitRef":"pricingUnits/0","type":"Unspecified"}]""";

        public static string InsertSql =>
            $"""
             INSERT INTO [Order].[OrderPreparations]
                 ([Id],[OwnerAirlineId],[FinancialCustomerId],[Channel],[CallerScope],[ActorContextType],[SourceOwner],[SourceOfferId],
                  [ProviderProfileId],[ContractVersion],[AcceptanceProfile],[AcceptanceAssurance],[SourcePayloadHash],[CanonicalizationVersion],
                  [SnapshotDigest],[CandidateJson],[PricedAt],[CapturedAt],[OfferValidityState],[OfferValidityOwner],[PriceValidityState],
                  [PriceValidityOwner],[TicketingValidityState],[TicketingValidityOwner],[CreatedAt],[LastUpdateTime])
             VALUES ({PreparationId},1,100,1,N'legacy',1,N'AirOffer',N'LEGACY-OFFER-1',N'LIVE-CANDIDATE-SANDBOX',N'legacy-contract',
                     N'LIVE-CANDIDATE-SANDBOX',2,REPLICATE('a',64),N'ordering-canonical-json-v1',REPLICATE('b',64),N'',
                     SYSDATETIMEOFFSET(),SYSDATETIMEOFFSET(),2,N'AirOffer',2,N'AirPrice',2,N'Unresolved owner',SYSDATETIMEOFFSET(),SYSDATETIMEOFFSET());

             INSERT INTO [Order].[Orders]
                 ([Id],[OrderReference],[RootOrderId],[OwnerAirlineId],[FinancialCustomerId],[Channel],[BuyerActorContextType],[SaleCurrencyRef],
                  [AcceptedPreparationId],[AcceptedSourceOwner],[AcceptedSourceOfferId],[AcceptedProviderProfileId],[AcceptedContractVersion],
                  [AcceptanceProfile],[AcceptanceAssurance],[AcceptedSnapshotDigest],[AcceptedSourcePayloadHash],[SourcePricedAt],[SourceCapturedAt],
                  [ClientAcceptedAt],[AcceptedAt],[SourcePreparationId],[OfferValidityState],[OfferValidityOwner],[PriceValidityState],
                  [PriceValidityOwner],[TicketingValidityState],[TicketingValidityOwner],[CommercialSummary],[CustomerTotalAmount],
                  [CustomerTotalCurrencyRef],[CommercialVersion],[FinancialSequence],[OrderRevision],[LastEventOrdinal],[CreatedAt],[LastUpdateTime])
             VALUES ({OrderId},N'LEGACY01',{OrderId},1,100,1,1,N'978',{PreparationId},N'AirOffer',N'LEGACY-OFFER-1',N'LIVE-CANDIDATE-SANDBOX',
                     N'legacy-contract',N'LIVE-CANDIDATE-SANDBOX',2,REPLICATE('b',64),REPLICATE('a',64),
                     '2026-09-01T10:00:00+00:00','2026-09-01T10:05:00+00:00','2026-09-01T10:06:00+00:00','2026-09-01T10:07:00+00:00',
                     {PreparationId},2,N'AirOffer',2,N'AirPrice',2,N'Unresolved owner',1,120.00,N'978',1,1,1,1,
                     '2026-09-01T10:07:00+00:00',SYSDATETIMEOFFSET());

             INSERT INTO [Order].[OrderChanges]
                 ([Id],[OrderId],[Type],[CommercialVersion],[ActorContextType],[SourceDecisionRef],[CommittedAt],[LastUpdateTime])
             VALUES ({ChangeId},{OrderId},1,1,1,N'preparation:{PreparationId}','2026-09-01T10:07:00+00:00',SYSDATETIMEOFFSET());

             INSERT INTO [Order].[PriceChangeSets]
                 ([Id],[OrderId],[ChangeId],[FinancialSequence],[Reason],[SourceDecisionRef],[BaseCommercialVersion],[CommittedAt],[LastUpdateTime])
             VALUES ({PriceChangeSetId},{OrderId},{ChangeId},1,1,N'preparation:{PreparationId}',0,'2026-09-01T10:07:00+00:00',SYSDATETIMEOFFSET());

             INSERT INTO [Order].[OrderTravelers]
                 ([Id],[OrderId],[SourceTravellerRef],[ClientTravelerRef],[PassengerTypeCode],[LastUpdateTime])
             VALUES ({TravelerId},{OrderId},N'PAX-A',N'CLIENT-A',1,SYSDATETIMEOFFSET());

             INSERT INTO [Order].[OrderSegments]
                 ([Id],[OrderId],[Sequence],[SourceSegmentRef],[Kind],[OriginRef],[DestinationRef],[LastUpdateTime])
             VALUES ({SegmentId},{OrderId},1,N'BOUND-1|9001',1,N'1001',N'1002',SYSDATETIMEOFFSET());

             INSERT INTO [Order].[OrderItems]
                 ([Id],[OrderId],[SourceItemRef],[Kind],[AcceptedTotalAmount],[AcceptedTotalCurrencyRef],[CreatedByChangeId],[CommercialStatus],[LastUpdateTime])
             VALUES ({ItemId},{OrderId},N'OFFER-PACKAGE',1,120.00,N'978',{ChangeId},1,SYSDATETIMEOFFSET());

             INSERT INTO [Order].[OrderServices]
                 ([Id],[OrderId],[OrderItemId],[SourceServiceRef],[Type],[CommercialStatus],[ServiceVersion],[Quantity],[QuantityUnit],
                  [DetailSchema],[DetailSchemaVersion],[FulfillmentProfileRef],[ReservationRequirement],[DocumentKind],[RequiresFunding],
                  [CapacityUnits],[CreatedByChangeId],[LastUpdateTime])
             VALUES ({ServiceId},{OrderId},{ItemId},N'PAX-A|BOUND-1|9001',1,1,1,1,1,N'AirTransport',1,
                     N'AIROFFER-OBSERVED-AIR-UNCERTIFIED',2,2,1,1,{ChangeId},SYSDATETIMEOFFSET());

             INSERT INTO [Order].[AirTransportServiceDetails]
                 ([ServiceId],[CabinRef],[RbdRef],[BookingClass],[FlightNumber],[FlightVersion],[MarketingCarrierRef],[OperatingCarrierRef])
             VALUES ({ServiceId},N'CABIN-1',N'RBD-1',N'Y',N'XX100',N'3',N'CARRIER-M',N'CARRIER-O');

             INSERT INTO [Order].[OrderServiceBeneficiaries] ([Id],[ServiceId],[TravelerId],[LastUpdateTime])
             VALUES ({ServiceId + 100},{ServiceId},{TravelerId},SYSDATETIMEOFFSET());

             INSERT INTO [Order].[OrderServiceCoverage] ([Id],[ServiceId],[SegmentId],[LastUpdateTime])
             VALUES ({ServiceId + 200},{ServiceId},{SegmentId},SYSDATETIMEOFFSET());

             INSERT INTO [Order].[OrderItemServiceLinks]
                 ([Id],[OrderIdAtAssociation],[OrderItemId],[OrderServiceId],[LinkedByChangeId],[LastUpdateTime])
             VALUES ({LinkId},{OrderId},{ItemId},{ServiceId},{ChangeId},SYSDATETIMEOFFSET());

             INSERT INTO [Order].[PricingLines]
                 ([Id],[OrderId],[PriceChangeSetId],[SourceLineRef],[CandidateLineRef],[Component],[Effect],[Direction],[Role],
                  [OriginalValueAmount],[OriginalValueCurrencyRef],[SaleValueAmount],[SaleValueCurrencyRef],[BasisType],[SourceBasisRef],[LastUpdateTime])
             VALUES ({PricingLineId},{OrderId},{PriceChangeSetId},N'tickets/0/coupons/0/pricings/0',N'tickets/0/coupons/0/pricings/0',
                     1,1,1,1,120.00,N'978',120.00,N'978',3,N'PAX-A|BOUND-1|9001',SYSDATETIMEOFFSET());

             INSERT INTO [Order].[FareConstructions]
                 ([Id],[OrderIdAtCreation],[CreatedByChangeId],[Assurance],[SourceContextRef],[PricingUnitsJson],[LastUpdateTime])
             VALUES ({FareConstructionId},{OrderId},{ChangeId},2,N'airoffer:details:pricingUnits',N'{PricingUnitsJson}',SYSDATETIMEOFFSET());
             """;
    }
}
