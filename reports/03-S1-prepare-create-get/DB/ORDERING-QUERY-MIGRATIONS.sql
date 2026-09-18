IF OBJECT_ID(N'[dbo].[__QueriesMigrationHistory]') IS NULL
BEGIN
    CREATE TABLE [dbo].[__QueriesMigrationHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___QueriesMigrationHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [dbo].[__QueriesMigrationHistory]
    WHERE [MigrationId] = N'20260917160511_S1OrderDetailsProjection'
)
BEGIN
    IF SCHEMA_ID(N'ReadModel') IS NULL EXEC(N'CREATE SCHEMA [ReadModel];');
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__QueriesMigrationHistory]
    WHERE [MigrationId] = N'20260917160511_S1OrderDetailsProjection'
)
BEGIN
    CREATE TABLE [ReadModel].[OrderDetails] (
        [OrderId] bigint NOT NULL,
        [OwnerAirlineId] bigint NOT NULL,
        [FinancialCustomerId] bigint NOT NULL,
        [OrderReference] nvarchar(64) NOT NULL,
        [OrderRevision] bigint NOT NULL,
        [CommercialVersion] int NOT NULL,
        [ProjectionSchemaVersion] int NOT NULL,
        [DetailsJson] nvarchar(max) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [ProjectedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_OrderDetails] PRIMARY KEY ([OrderId]),
        CONSTRAINT [CK_OrderDetails_Json] CHECK (ISJSON([DetailsJson]) = 1),
        CONSTRAINT [CK_OrderDetails_Revision] CHECK ([OrderRevision] >= 1 AND [ProjectionSchemaVersion] >= 1)
    );
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__QueriesMigrationHistory]
    WHERE [MigrationId] = N'20260917160511_S1OrderDetailsProjection'
)
BEGIN
    CREATE INDEX [IX_OrderDetails_OwnerAirlineId_FinancialCustomerId_CreatedAt] ON [ReadModel].[OrderDetails] ([OwnerAirlineId], [FinancialCustomerId], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__QueriesMigrationHistory]
    WHERE [MigrationId] = N'20260917160511_S1OrderDetailsProjection'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrderDetails_OwnerAirlineId_OrderReference] ON [ReadModel].[OrderDetails] ([OwnerAirlineId], [OrderReference]);
END;

IF NOT EXISTS (
    SELECT * FROM [dbo].[__QueriesMigrationHistory]
    WHERE [MigrationId] = N'20260917160511_S1OrderDetailsProjection'
)
BEGIN
    INSERT INTO [dbo].[__QueriesMigrationHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260917160511_S1OrderDetailsProjection', N'10.0.9');
END;

COMMIT;
GO

