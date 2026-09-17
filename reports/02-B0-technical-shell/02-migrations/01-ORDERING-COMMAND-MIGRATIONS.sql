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

