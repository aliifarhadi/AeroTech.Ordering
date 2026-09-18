IF OBJECT_ID(N'[dbo].[__CommandsMigrationHistory]') IS NULL
BEGIN
    CREATE TABLE [dbo].[__CommandsMigrationHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___CommandsMigrationHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917144148_B0InfrastructureShell'
)
BEGIN
    CREATE TABLE [dbo].[InboxMessages] (
        [OwnerAirlineId] bigint NOT NULL,
        [SourceSystem] nvarchar(64) NOT NULL,
        [EventId] nvarchar(64) NOT NULL,
        [Consumer] nvarchar(256) NOT NULL,
        [MessageType] nvarchar(500) NOT NULL,
        [PayloadHash] char(64) NOT NULL,
        [ReceivedOn] datetimeoffset NOT NULL,
        CONSTRAINT [PK_InboxMessages] PRIMARY KEY ([OwnerAirlineId], [SourceSystem], [EventId], [Consumer])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917144148_B0InfrastructureShell'
)
BEGIN
    CREATE TABLE [dbo].[OutboxMessages] (
        [Id] bigint NOT NULL IDENTITY,
        [EventId] nvarchar(64) NOT NULL,
        [MessageType] nvarchar(500) NOT NULL,
        [Payload] nvarchar(max) NOT NULL,
        [OccurredOn] datetimeoffset NOT NULL,
        [ProcessedOn] datetimeoffset NULL,
        [AttemptCount] int NOT NULL,
        [LastError] nvarchar(2000) NULL,
        [LeaseOwner] nvarchar(128) NULL,
        [LeaseExpiresOn] datetimeoffset NULL,
        [LeaseVersion] bigint NOT NULL,
        CONSTRAINT [PK_OutboxMessages] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917144148_B0InfrastructureShell'
)
BEGIN
    CREATE INDEX [IX_InboxMessages_ReceivedOn] ON [dbo].[InboxMessages] ([ReceivedOn]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917144148_B0InfrastructureShell'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OutboxMessages_EventId] ON [dbo].[OutboxMessages] ([EventId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917144148_B0InfrastructureShell'
)
BEGIN
    CREATE INDEX [IX_OutboxMessages_ProcessedOn_LeaseExpiresOn] ON [dbo].[OutboxMessages] ([ProcessedOn], [LeaseExpiresOn]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917144148_B0InfrastructureShell'
)
BEGIN
    INSERT INTO [dbo].[__CommandsMigrationHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260917144148_B0InfrastructureShell', N'10.0.9');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    IF SCHEMA_ID(N'Commercial') IS NULL EXEC(N'CREATE SCHEMA [Commercial];');
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    IF SCHEMA_ID(N'Operations') IS NULL EXEC(N'CREATE SCHEMA [Operations];');
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    ALTER TABLE [dbo].[OutboxMessages] ADD [EventOrdinal] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    ALTER TABLE [dbo].[OutboxMessages] ADD [StreamId] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    ALTER TABLE [dbo].[OutboxMessages] ADD [StreamKind] nvarchar(32) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE TABLE [Operations].[CommandReceipts] (
        [Id] bigint NOT NULL,
        [OwnerAirlineId] bigint NOT NULL,
        [FinancialCustomerId] bigint NOT NULL,
        [CallerScope] nvarchar(256) NOT NULL,
        [CommandKind] int NOT NULL,
        [IdempotencyKey] nvarchar(128) NOT NULL,
        [CanonicalizationVersion] nvarchar(64) NOT NULL,
        [RequestDigest] char(64) NOT NULL,
        [OperationId] bigint NOT NULL,
        [Status] int NOT NULL,
        [PreparationId] bigint NULL,
        [OrderId] bigint NULL,
        [ResultJson] nvarchar(max) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CompletedAt] datetimeoffset NULL,
        [LastUpdateTime] datetimeoffset NOT NULL,
        [LastUpdatedBy] bigint NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_CommandReceipts] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE TABLE [Commercial].[OrderPreparations] (
        [Id] bigint NOT NULL,
        [OwnerAirlineId] bigint NOT NULL,
        [FinancialCustomerId] bigint NOT NULL,
        [Channel] nvarchar(64) NOT NULL,
        [SellingOfficeId] bigint NULL,
        [CallerScope] nvarchar(256) NOT NULL,
        [ActorContextType] int NOT NULL,
        [ActorId] bigint NULL,
        [SourceOwner] nvarchar(64) NOT NULL,
        [SourceOfferId] nvarchar(2048) NOT NULL,
        [ProviderProfileId] nvarchar(64) NOT NULL,
        [ContractVersion] nvarchar(64) NOT NULL,
        [AcceptanceProfile] nvarchar(64) NOT NULL,
        [AcceptanceAssurance] int NOT NULL,
        [OwnerBindingRef] nvarchar(128) NULL,
        [SourcePayloadHash] nvarchar(64) NOT NULL,
        [CanonicalizationVersion] nvarchar(64) NOT NULL,
        [SnapshotDigest] char(64) NOT NULL,
        [CandidateJson] nvarchar(max) NOT NULL,
        [PricedAt] datetimeoffset NOT NULL,
        [CapturedAt] datetimeoffset NOT NULL,
        [OfferValidityState] int NOT NULL,
        [OfferValidityValue] datetimeoffset NULL,
        [OfferValidityOwner] nvarchar(64) NOT NULL,
        [OfferValiditySourceRef] nvarchar(128) NULL,
        [OfferValidityReason] nvarchar(512) NULL,
        [PriceValidityState] int NOT NULL,
        [PriceValidityValue] datetimeoffset NULL,
        [PriceValidityOwner] nvarchar(64) NOT NULL,
        [PriceValiditySourceRef] nvarchar(128) NULL,
        [PriceValidityReason] nvarchar(512) NULL,
        [TicketingValidityState] int NOT NULL,
        [TicketingValidityValue] datetimeoffset NULL,
        [TicketingValidityOwner] nvarchar(64) NOT NULL,
        [TicketingValiditySourceRef] nvarchar(128) NULL,
        [TicketingValidityReason] nvarchar(512) NULL,
        [ClientReference] nvarchar(128) NULL,
        [ConsumedByOrderId] bigint NULL,
        [ConsumedAt] datetimeoffset NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [LastUpdateTime] datetimeoffset NOT NULL,
        [LastUpdatedBy] bigint NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_OrderPreparations] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_OrderPreparations_Consumption] CHECK (([ConsumedByOrderId] IS NULL AND [ConsumedAt] IS NULL) OR ([ConsumedByOrderId] IS NOT NULL AND [ConsumedAt] IS NOT NULL)),
        CONSTRAINT [CK_OrderPreparations_Digest] CHECK (LEN([SnapshotDigest]) = 64)
    );
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE TABLE [Commercial].[Orders] (
        [Id] bigint NOT NULL,
        [OrderReference] nvarchar(64) NOT NULL,
        [RootOrderId] bigint NOT NULL,
        [OwnerAirlineId] bigint NOT NULL,
        [FinancialCustomerId] bigint NOT NULL,
        [Channel] nvarchar(64) NOT NULL,
        [SellingOfficeId] bigint NULL,
        [BuyerActorContextType] int NOT NULL,
        [BuyerActorId] bigint NULL,
        [SaleCurrencyRef] nvarchar(32) NOT NULL,
        [AcceptedPreparationId] bigint NOT NULL,
        [AcceptedSourceOwner] nvarchar(64) NOT NULL,
        [AcceptedSourceOfferId] nvarchar(2048) NOT NULL,
        [AcceptedProviderProfileId] nvarchar(64) NOT NULL,
        [AcceptedContractVersion] nvarchar(64) NOT NULL,
        [AcceptanceProfile] nvarchar(64) NOT NULL,
        [AcceptanceAssurance] int NOT NULL,
        [AcceptedOwnerBindingRef] nvarchar(128) NULL,
        [AcceptedSnapshotDigest] char(64) NOT NULL,
        [AcceptedSourcePayloadHash] nvarchar(64) NOT NULL,
        [SourcePricedAt] datetimeoffset NOT NULL,
        [SourceCapturedAt] datetimeoffset NOT NULL,
        [ClientAcceptedAt] datetimeoffset NOT NULL,
        [AcceptedAt] datetimeoffset NOT NULL,
        [SourcePreparationId] bigint NOT NULL,
        [OfferValidityState] int NOT NULL,
        [OfferValidityValue] datetimeoffset NULL,
        [OfferValidityOwner] nvarchar(64) NOT NULL,
        [OfferValiditySourceRef] nvarchar(128) NULL,
        [OfferValidityReason] nvarchar(512) NULL,
        [PriceValidityState] int NOT NULL,
        [PriceValidityValue] datetimeoffset NULL,
        [PriceValidityOwner] nvarchar(64) NOT NULL,
        [PriceValiditySourceRef] nvarchar(128) NULL,
        [PriceValidityReason] nvarchar(512) NULL,
        [TicketingValidityState] int NOT NULL,
        [TicketingValidityValue] datetimeoffset NULL,
        [TicketingValidityOwner] nvarchar(64) NOT NULL,
        [TicketingValiditySourceRef] nvarchar(128) NULL,
        [TicketingValidityReason] nvarchar(512) NULL,
        [CommercialSummary] int NOT NULL,
        [CustomerTotalAmount] decimal(28,8) NOT NULL,
        [CustomerTotalCurrencyRef] nvarchar(32) NOT NULL,
        [CommercialVersion] int NOT NULL,
        [FinancialSequence] int NOT NULL,
        [OrderRevision] bigint NOT NULL,
        [LastEventOrdinal] bigint NOT NULL,
        [ClientReference] nvarchar(128) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [LastUpdateTime] datetimeoffset NOT NULL,
        [LastUpdatedBy] bigint NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Orders_Revisions] CHECK ([CommercialVersion] >= 1 AND [FinancialSequence] >= 0 AND [OrderRevision] >= 1 AND [LastEventOrdinal] >= 0),
        CONSTRAINT [CK_Orders_Root] CHECK ([RootOrderId] = [Id]),
        CONSTRAINT [FK_Orders_OrderPreparations_SourcePreparationId] FOREIGN KEY ([SourcePreparationId]) REFERENCES [Commercial].[OrderPreparations] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE TABLE [Commercial].[PreparationSourceEvidence] (
        [Id] bigint NOT NULL,
        [PreparationId] bigint NOT NULL,
        [EvidenceRef] nvarchar(128) NOT NULL,
        [PayloadHash] nvarchar(64) NOT NULL,
        [ContentType] nvarchar(64) NOT NULL,
        [Payload] nvarchar(max) NOT NULL,
        [CapturedAt] datetimeoffset NOT NULL,
        [LastUpdateTime] datetimeoffset NOT NULL,
        [LastUpdatedBy] bigint NULL,
        CONSTRAINT [PK_PreparationSourceEvidence] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PreparationSourceEvidence_OrderPreparations_PreparationId] FOREIGN KEY ([PreparationId]) REFERENCES [Commercial].[OrderPreparations] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE TABLE [Commercial].[OrderChanges] (
        [Id] bigint NOT NULL,
        [OrderId] bigint NOT NULL,
        [Type] int NOT NULL,
        [CommercialVersion] int NOT NULL,
        [ActorContextType] int NOT NULL,
        [ActorId] bigint NULL,
        [SourceDecisionRef] nvarchar(256) NOT NULL,
        [CommittedAt] datetimeoffset NOT NULL,
        [LastUpdateTime] datetimeoffset NOT NULL,
        [LastUpdatedBy] bigint NULL,
        CONSTRAINT [PK_OrderChanges] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderChanges_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Commercial].[Orders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE TABLE [Commercial].[OrderContacts] (
        [Id] bigint NOT NULL,
        [OrderId] bigint NOT NULL,
        [Sequence] int NOT NULL,
        [Role] int NOT NULL,
        [Email] nvarchar(256) NULL,
        [Phone] nvarchar(64) NULL,
        [LastUpdateTime] datetimeoffset NOT NULL,
        [LastUpdatedBy] bigint NULL,
        CONSTRAINT [PK_OrderContacts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderContacts_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Commercial].[Orders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE TABLE [Commercial].[OrderSegments] (
        [Id] bigint NOT NULL,
        [OrderId] bigint NOT NULL,
        [Sequence] int NOT NULL,
        [SourceSegmentRef] nvarchar(128) NOT NULL,
        [Kind] int NOT NULL,
        [OriginRef] nvarchar(128) NOT NULL,
        [DestinationRef] nvarchar(128) NOT NULL,
        [SoldDeparture] datetimeoffset NULL,
        [SoldArrival] datetimeoffset NULL,
        [FlightRef] nvarchar(128) NULL,
        [LastUpdateTime] datetimeoffset NOT NULL,
        [LastUpdatedBy] bigint NULL,
        CONSTRAINT [PK_OrderSegments] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_OrderSegments_Sequence] CHECK ([Sequence] >= 1),
        CONSTRAINT [FK_OrderSegments_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Commercial].[Orders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE TABLE [Commercial].[OrderTravelers] (
        [Id] bigint NOT NULL,
        [OrderId] bigint NOT NULL,
        [SourceTravellerRef] nvarchar(128) NOT NULL,
        [ClientTravelerRef] nvarchar(128) NOT NULL,
        [PassengerTypeCode] nvarchar(8) NOT NULL,
        [InfantParentTravelerId] bigint NULL,
        [LastUpdateTime] datetimeoffset NOT NULL,
        [LastUpdatedBy] bigint NULL,
        CONSTRAINT [PK_OrderTravelers] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_OrderTravelers_Guardian] CHECK ([InfantParentTravelerId] IS NULL OR [InfantParentTravelerId] <> [Id]),
        CONSTRAINT [FK_OrderTravelers_OrderTravelers_InfantParentTravelerId] FOREIGN KEY ([InfantParentTravelerId]) REFERENCES [Commercial].[OrderTravelers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrderTravelers_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Commercial].[Orders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE TABLE [Commercial].[FareConstructions] (
        [Id] bigint NOT NULL,
        [OrderIdAtCreation] bigint NOT NULL,
        [CreatedByChangeId] bigint NOT NULL,
        [SupersededByConstructionId] bigint NULL,
        [Assurance] int NOT NULL,
        [SourceContextRef] nvarchar(128) NOT NULL,
        [PricingUnitsJson] nvarchar(max) NOT NULL,
        [LastUpdateTime] datetimeoffset NOT NULL,
        [LastUpdatedBy] bigint NULL,
        CONSTRAINT [PK_FareConstructions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FareConstructions_FareConstructions_SupersededByConstructionId] FOREIGN KEY ([SupersededByConstructionId]) REFERENCES [Commercial].[FareConstructions] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FareConstructions_OrderChanges_CreatedByChangeId] FOREIGN KEY ([CreatedByChangeId]) REFERENCES [Commercial].[OrderChanges] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FareConstructions_Orders_OrderIdAtCreation] FOREIGN KEY ([OrderIdAtCreation]) REFERENCES [Commercial].[Orders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE TABLE [Commercial].[OrderItems] (
        [Id] bigint NOT NULL,
        [OrderId] bigint NOT NULL,
        [SourceItemRef] nvarchar(128) NOT NULL,
        [Kind] int NOT NULL,
        [SourceOfferItemRef] nvarchar(128) NULL,
        [AcceptedTotalAmount] decimal(28,8) NOT NULL,
        [AcceptedTotalCurrencyRef] nvarchar(32) NOT NULL,
        [CreatedByChangeId] bigint NOT NULL,
        [CommercialStatus] int NOT NULL,
        [LastUpdateTime] datetimeoffset NOT NULL,
        [LastUpdatedBy] bigint NULL,
        CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderItems_OrderChanges_CreatedByChangeId] FOREIGN KEY ([CreatedByChangeId]) REFERENCES [Commercial].[OrderChanges] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Commercial].[Orders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE TABLE [Commercial].[PriceChangeSets] (
        [Id] bigint NOT NULL,
        [OrderId] bigint NOT NULL,
        [ChangeId] bigint NOT NULL,
        [FinancialSequence] int NOT NULL,
        [Reason] int NOT NULL,
        [SourceDecisionRef] nvarchar(256) NOT NULL,
        [BaseCommercialVersion] int NOT NULL,
        [CommittedAt] datetimeoffset NOT NULL,
        [LastUpdateTime] datetimeoffset NOT NULL,
        [LastUpdatedBy] bigint NULL,
        CONSTRAINT [PK_PriceChangeSets] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_PriceChangeSets_Sequence] CHECK ([FinancialSequence] >= 1),
        CONSTRAINT [FK_PriceChangeSets_OrderChanges_ChangeId] FOREIGN KEY ([ChangeId]) REFERENCES [Commercial].[OrderChanges] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PriceChangeSets_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Commercial].[Orders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE TABLE [Commercial].[OrderSegmentLegs] (
        [Id] bigint NOT NULL,
        [SegmentId] bigint NOT NULL,
        [Sequence] int NOT NULL,
        [SourceLegRef] nvarchar(128) NOT NULL,
        [LastUpdateTime] datetimeoffset NOT NULL,
        [LastUpdatedBy] bigint NULL,
        CONSTRAINT [PK_OrderSegmentLegs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderSegmentLegs_OrderSegments_SegmentId] FOREIGN KEY ([SegmentId]) REFERENCES [Commercial].[OrderSegments] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE TABLE [Commercial].[OrderTravelerIdentities] (
        [TravelerId] bigint NOT NULL,
        [GivenName] nvarchar(128) NOT NULL,
        [Surname] nvarchar(128) NOT NULL,
        [DateOfBirth] date NOT NULL,
        CONSTRAINT [PK_OrderTravelerIdentities] PRIMARY KEY ([TravelerId]),
        CONSTRAINT [FK_OrderTravelerIdentities_OrderTravelers_TravelerId] FOREIGN KEY ([TravelerId]) REFERENCES [Commercial].[OrderTravelers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE TABLE [Commercial].[FundingObligations] (
        [Id] bigint NOT NULL,
        [OrderId] bigint NOT NULL,
        [Version] int NOT NULL,
        [Purpose] int NOT NULL,
        [AmountAmount] decimal(28,8) NOT NULL,
        [AmountCurrencyRef] nvarchar(32) NOT NULL,
        [OrderItemId] bigint NULL,
        [ChangeId] bigint NOT NULL,
        [SourceDecisionRef] nvarchar(256) NOT NULL,
        [SupersededObligationId] bigint NULL,
        [LastUpdateTime] datetimeoffset NOT NULL,
        [LastUpdatedBy] bigint NULL,
        CONSTRAINT [PK_FundingObligations] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_FundingObligations_Amount] CHECK ([AmountAmount] >= 0),
        CONSTRAINT [CK_FundingObligations_Version] CHECK ([Version] >= 1),
        CONSTRAINT [FK_FundingObligations_FundingObligations_SupersededObligationId] FOREIGN KEY ([SupersededObligationId]) REFERENCES [Commercial].[FundingObligations] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FundingObligations_OrderChanges_ChangeId] FOREIGN KEY ([ChangeId]) REFERENCES [Commercial].[OrderChanges] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FundingObligations_OrderItems_OrderItemId] FOREIGN KEY ([OrderItemId]) REFERENCES [Commercial].[OrderItems] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FundingObligations_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Commercial].[Orders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE TABLE [Commercial].[OrderServices] (
        [Id] bigint NOT NULL,
        [OrderId] bigint NOT NULL,
        [OrderItemId] bigint NOT NULL,
        [SourceServiceRef] nvarchar(128) NOT NULL,
        [Type] int NOT NULL,
        [CommercialStatus] int NOT NULL,
        [ServiceVersion] int NOT NULL,
        [Quantity] decimal(18,6) NOT NULL,
        [QuantityUnit] nvarchar(64) NOT NULL,
        [DetailSchema] nvarchar(64) NOT NULL,
        [DetailSchemaVersion] int NOT NULL,
        [FulfillmentProfileRef] nvarchar(64) NOT NULL,
        [ReservationRequirement] int NOT NULL,
        [DocumentKind] int NOT NULL,
        [RequiresFunding] bit NOT NULL,
        [CapacityUnits] int NOT NULL,
        [CreatedByChangeId] bigint NOT NULL,
        [LastUpdateTime] datetimeoffset NOT NULL,
        [LastUpdatedBy] bigint NULL,
        CONSTRAINT [PK_OrderServices] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_OrderServices_Quantity] CHECK ([Quantity] > 0),
        CONSTRAINT [CK_OrderServices_Version] CHECK ([ServiceVersion] >= 1),
        CONSTRAINT [FK_OrderServices_OrderChanges_CreatedByChangeId] FOREIGN KEY ([CreatedByChangeId]) REFERENCES [Commercial].[OrderChanges] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrderServices_OrderItems_OrderItemId] FOREIGN KEY ([OrderItemId]) REFERENCES [Commercial].[OrderItems] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrderServices_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Commercial].[Orders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE TABLE [Commercial].[PricingLines] (
        [Id] bigint NOT NULL,
        [OrderId] bigint NOT NULL,
        [PriceChangeSetId] bigint NOT NULL,
        [OrderItemId] bigint NULL,
        [SourceLineRef] nvarchar(512) NOT NULL,
        [CandidateLineRef] nvarchar(128) NOT NULL,
        [Component] int NOT NULL,
        [Effect] int NOT NULL,
        [Direction] int NOT NULL,
        [Role] int NOT NULL,
        [OriginalValueAmount] decimal(28,8) NOT NULL,
        [OriginalValueCurrencyRef] nvarchar(32) NOT NULL,
        [SaleValueAmount] decimal(28,8) NOT NULL,
        [SaleValueCurrencyRef] nvarchar(32) NOT NULL,
        [BasisType] int NOT NULL,
        [BasisId] bigint NULL,
        [SourceBasisRef] nvarchar(128) NOT NULL,
        [SourceConversionRef] nvarchar(128) NULL,
        [OriginalPricingLineId] bigint NULL,
        [LastUpdateTime] datetimeoffset NOT NULL,
        [LastUpdatedBy] bigint NULL,
        CONSTRAINT [PK_PricingLines] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_PricingLines_CommissionNotCustomer] CHECK (NOT ([Component] = 9 AND [Effect] = 1)),
        CONSTRAINT [CK_PricingLines_Direction] CHECK ([Direction] IN (1, 2)),
        CONSTRAINT [CK_PricingLines_OriginalMagnitude] CHECK ([OriginalValueAmount] >= 0),
        CONSTRAINT [CK_PricingLines_OtherInformational] CHECK ([Component] <> 11 OR [Effect] = 3),
        CONSTRAINT [CK_PricingLines_ReversalReference] CHECK ([Role] <> 2 OR [OriginalPricingLineId] IS NOT NULL),
        CONSTRAINT [CK_PricingLines_SaleMagnitude] CHECK ([SaleValueAmount] >= 0),
        CONSTRAINT [CK_PricingLines_TaxNotSettlement] CHECK (NOT ([Component] = 3 AND [Effect] = 2)),
        CONSTRAINT [FK_PricingLines_OrderItems_OrderItemId] FOREIGN KEY ([OrderItemId]) REFERENCES [Commercial].[OrderItems] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PricingLines_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Commercial].[Orders] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PricingLines_PriceChangeSets_PriceChangeSetId] FOREIGN KEY ([PriceChangeSetId]) REFERENCES [Commercial].[PriceChangeSets] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PricingLines_PricingLines_OriginalPricingLineId] FOREIGN KEY ([OriginalPricingLineId]) REFERENCES [Commercial].[PricingLines] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE TABLE [Commercial].[AirTransportServiceDetails] (
        [ServiceId] bigint NOT NULL,
        [CabinRef] nvarchar(128) NULL,
        [RbdRef] nvarchar(128) NULL,
        [BookingClass] nvarchar(128) NULL,
        [FlightNumber] nvarchar(128) NULL,
        [FlightVersion] nvarchar(128) NULL,
        [MarketingCarrierRef] nvarchar(128) NULL,
        [OperatingCarrierRef] nvarchar(128) NULL,
        CONSTRAINT [PK_AirTransportServiceDetails] PRIMARY KEY ([ServiceId]),
        CONSTRAINT [FK_AirTransportServiceDetails_OrderServices_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Commercial].[OrderServices] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE TABLE [Commercial].[OrderItemServiceLinks] (
        [Id] bigint NOT NULL,
        [OrderIdAtAssociation] bigint NOT NULL,
        [OrderItemId] bigint NOT NULL,
        [OrderServiceId] bigint NOT NULL,
        [LinkedByChangeId] bigint NOT NULL,
        [LastUpdateTime] datetimeoffset NOT NULL,
        [LastUpdatedBy] bigint NULL,
        CONSTRAINT [PK_OrderItemServiceLinks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderItemServiceLinks_OrderChanges_LinkedByChangeId] FOREIGN KEY ([LinkedByChangeId]) REFERENCES [Commercial].[OrderChanges] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrderItemServiceLinks_OrderItems_OrderItemId] FOREIGN KEY ([OrderItemId]) REFERENCES [Commercial].[OrderItems] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrderItemServiceLinks_OrderServices_OrderServiceId] FOREIGN KEY ([OrderServiceId]) REFERENCES [Commercial].[OrderServices] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrderItemServiceLinks_Orders_OrderIdAtAssociation] FOREIGN KEY ([OrderIdAtAssociation]) REFERENCES [Commercial].[Orders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE TABLE [Commercial].[OrderServiceBeneficiaries] (
        [Id] bigint NOT NULL,
        [ServiceId] bigint NOT NULL,
        [TravelerId] bigint NOT NULL,
        [LastUpdateTime] datetimeoffset NOT NULL,
        [LastUpdatedBy] bigint NULL,
        CONSTRAINT [PK_OrderServiceBeneficiaries] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderServiceBeneficiaries_OrderServices_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Commercial].[OrderServices] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrderServiceBeneficiaries_OrderTravelers_TravelerId] FOREIGN KEY ([TravelerId]) REFERENCES [Commercial].[OrderTravelers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE TABLE [Commercial].[OrderServiceCoverage] (
        [Id] bigint NOT NULL,
        [ServiceId] bigint NOT NULL,
        [SegmentId] bigint NOT NULL,
        [LastUpdateTime] datetimeoffset NOT NULL,
        [LastUpdatedBy] bigint NULL,
        CONSTRAINT [PK_OrderServiceCoverage] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderServiceCoverage_OrderSegments_SegmentId] FOREIGN KEY ([SegmentId]) REFERENCES [Commercial].[OrderSegments] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrderServiceCoverage_OrderServices_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Commercial].[OrderServices] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_OutboxMessages_Stream_EventOrdinal] ON [dbo].[OutboxMessages] ([StreamKind], [StreamId], [EventOrdinal]) WHERE [StreamKind] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    EXEC(N'ALTER TABLE [dbo].[OutboxMessages] ADD CONSTRAINT [CK_OutboxMessages_Stream] CHECK (([StreamKind] IS NULL AND [StreamId] IS NULL AND [EventOrdinal] IS NULL) OR ([StreamKind] IS NOT NULL AND [StreamId] IS NOT NULL AND [EventOrdinal] >= 1))');
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CommandReceipts_OperationId] ON [Operations].[CommandReceipts] ([OperationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_CommandReceipts_OwnerAirlineId_OrderId] ON [Operations].[CommandReceipts] ([OwnerAirlineId], [OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UX_CommandReceipts_Scope_Key] ON [Operations].[CommandReceipts] ([OwnerAirlineId], [CallerScope], [CommandKind], [IdempotencyKey]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_FareConstructions_CreatedByChangeId] ON [Commercial].[FareConstructions] ([CreatedByChangeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_FareConstructions_SupersededByConstructionId] ON [Commercial].[FareConstructions] ([SupersededByConstructionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_FareConstructions_CurrentPerOrder] ON [Commercial].[FareConstructions] ([OrderIdAtCreation]) WHERE [SupersededByConstructionId] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_FundingObligations_ChangeId] ON [Commercial].[FundingObligations] ([ChangeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FundingObligations_OrderId_Id_Version] ON [Commercial].[FundingObligations] ([OrderId], [Id], [Version]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_FundingObligations_OrderItemId] ON [Commercial].[FundingObligations] ([OrderItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_FundingObligations_SupersededObligationId] ON [Commercial].[FundingObligations] ([SupersededObligationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrderChanges_OrderId_CommercialVersion] ON [Commercial].[OrderChanges] ([OrderId], [CommercialVersion]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrderContacts_OrderId_Sequence] ON [Commercial].[OrderContacts] ([OrderId], [Sequence]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderItems_CreatedByChangeId] ON [Commercial].[OrderItems] ([CreatedByChangeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderItems_OrderId_CommercialStatus] ON [Commercial].[OrderItems] ([OrderId], [CommercialStatus]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrderItems_OrderId_SourceItemRef] ON [Commercial].[OrderItems] ([OrderId], [SourceItemRef]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderItemServiceLinks_LinkedByChangeId] ON [Commercial].[OrderItemServiceLinks] ([LinkedByChangeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderItemServiceLinks_OrderIdAtAssociation] ON [Commercial].[OrderItemServiceLinks] ([OrderIdAtAssociation]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrderItemServiceLinks_OrderItemId_OrderServiceId_LinkedByChangeId] ON [Commercial].[OrderItemServiceLinks] ([OrderItemId], [OrderServiceId], [LinkedByChangeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderItemServiceLinks_OrderServiceId] ON [Commercial].[OrderItemServiceLinks] ([OrderServiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderPreparations_ConsumedAt_CreatedAt] ON [Commercial].[OrderPreparations] ([ConsumedAt], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderPreparations_OwnerAirlineId_FinancialCustomerId_CreatedAt] ON [Commercial].[OrderPreparations] ([OwnerAirlineId], [FinancialCustomerId], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_OrderPreparations_ConsumedByOrderId] ON [Commercial].[OrderPreparations] ([ConsumedByOrderId]) WHERE [ConsumedByOrderId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_OwnerAirlineId_CommercialSummary] ON [Commercial].[Orders] ([OwnerAirlineId], [CommercialSummary]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_OwnerAirlineId_FinancialCustomerId_CreatedAt] ON [Commercial].[Orders] ([OwnerAirlineId], [FinancialCustomerId], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_SourcePreparationId] ON [Commercial].[Orders] ([SourcePreparationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UX_Orders_Owner_Reference] ON [Commercial].[Orders] ([OwnerAirlineId], [OrderReference]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UX_Orders_Owner_SourcePreparation] ON [Commercial].[Orders] ([OwnerAirlineId], [SourcePreparationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrderSegmentLegs_SegmentId_Sequence] ON [Commercial].[OrderSegmentLegs] ([SegmentId], [Sequence]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrderSegments_OrderId_Sequence] ON [Commercial].[OrderSegments] ([OrderId], [Sequence]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrderSegments_OrderId_SourceSegmentRef] ON [Commercial].[OrderSegments] ([OrderId], [SourceSegmentRef]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrderServiceBeneficiaries_ServiceId_TravelerId] ON [Commercial].[OrderServiceBeneficiaries] ([ServiceId], [TravelerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderServiceBeneficiaries_TravelerId] ON [Commercial].[OrderServiceBeneficiaries] ([TravelerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderServiceCoverage_SegmentId] ON [Commercial].[OrderServiceCoverage] ([SegmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrderServiceCoverage_ServiceId_SegmentId] ON [Commercial].[OrderServiceCoverage] ([ServiceId], [SegmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderServices_CreatedByChangeId] ON [Commercial].[OrderServices] ([CreatedByChangeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrderServices_OrderId_SourceServiceRef] ON [Commercial].[OrderServices] ([OrderId], [SourceServiceRef]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderServices_OrderItemId_CommercialStatus] ON [Commercial].[OrderServices] ([OrderItemId], [CommercialStatus]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderTravelers_InfantParentTravelerId] ON [Commercial].[OrderTravelers] ([InfantParentTravelerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrderTravelers_OrderId_ClientTravelerRef] ON [Commercial].[OrderTravelers] ([OrderId], [ClientTravelerRef]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrderTravelers_OrderId_SourceTravellerRef] ON [Commercial].[OrderTravelers] ([OrderId], [SourceTravellerRef]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PreparationSourceEvidence_PreparationId_EvidenceRef] ON [Commercial].[PreparationSourceEvidence] ([PreparationId], [EvidenceRef]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PriceChangeSets_ChangeId] ON [Commercial].[PriceChangeSets] ([ChangeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PriceChangeSets_OrderId_FinancialSequence] ON [Commercial].[PriceChangeSets] ([OrderId], [FinancialSequence]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_PricingLines_OrderId_OrderItemId] ON [Commercial].[PricingLines] ([OrderId], [OrderItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_PricingLines_OrderItemId] ON [Commercial].[PricingLines] ([OrderItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE INDEX [IX_PricingLines_OriginalPricingLineId] ON [Commercial].[PricingLines] ([OriginalPricingLineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PricingLines_PriceChangeSetId_CandidateLineRef] ON [Commercial].[PricingLines] ([PriceChangeSetId], [CandidateLineRef]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__CommandsMigrationHistory]
    WHERE [MigrationId] = N'20260917160501_S1CommercialCreate'
)
BEGIN
    INSERT INTO [dbo].[__CommandsMigrationHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260917160501_S1CommercialCreate', N'10.0.9');
END;

COMMIT;
GO

