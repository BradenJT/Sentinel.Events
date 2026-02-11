IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Tenants] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [ApiKey] nvarchar(100) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_Tenants] PRIMARY KEY ([Id])
);

CREATE TABLE [Devices] (
    [Id] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [DeviceId] nvarchar(100) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Status] int NOT NULL,
    [LastSeenAt] datetimeoffset NOT NULL,
    [Metadata] nvarchar(max) NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_Devices] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Devices_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Alerts] (
    [Id] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [DeviceId] uniqueidentifier NOT NULL,
    [Type] int NOT NULL,
    [Severity] int NOT NULL,
    [Message] nvarchar(1000) NOT NULL,
    [IsPublic] bit NOT NULL,
    [IsAcknowledged] bit NOT NULL,
    [AcknowledgedAt] datetimeoffset NULL,
    [Metadata] nvarchar(max) NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_Alerts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Alerts_Devices_DeviceId] FOREIGN KEY ([DeviceId]) REFERENCES [Devices] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Alerts_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id])
);

CREATE TABLE [TelemetryEvents] (
    [Id] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [DeviceId] uniqueidentifier NOT NULL,
    [EventType] nvarchar(100) NOT NULL,
    [Payload] nvarchar(max) NOT NULL,
    [ReceivedAt] datetimeoffset NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_TelemetryEvents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TelemetryEvents_Devices_DeviceId] FOREIGN KEY ([DeviceId]) REFERENCES [Devices] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TelemetryEvents_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id])
);

CREATE TABLE [CommunityPosts] (
    [Id] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [AlertId] uniqueidentifier NULL,
    [Title] nvarchar(200) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [Location] nvarchar(200) NOT NULL,
    [ViewCount] int NOT NULL DEFAULT 0,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_CommunityPosts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CommunityPosts_Alerts_AlertId] FOREIGN KEY ([AlertId]) REFERENCES [Alerts] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_CommunityPosts_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id])
);

CREATE INDEX [IX_Alerts_CreatedAt] ON [Alerts] ([CreatedAt]);

CREATE INDEX [IX_Alerts_DeviceId] ON [Alerts] ([DeviceId]);

CREATE INDEX [IX_Alerts_Severity] ON [Alerts] ([Severity]);

CREATE INDEX [IX_Alerts_TenantId_DeviceId_Type_CreatedAt] ON [Alerts] ([TenantId], [DeviceId], [Type], [CreatedAt]);

CREATE INDEX [IX_CommunityPosts_AlertId] ON [CommunityPosts] ([AlertId]);

CREATE INDEX [IX_CommunityPosts_CreatedAt] ON [CommunityPosts] ([CreatedAt]);

CREATE INDEX [IX_CommunityPosts_TenantId] ON [CommunityPosts] ([TenantId]);

CREATE INDEX [IX_Devices_LastSeenAt] ON [Devices] ([LastSeenAt]);

CREATE INDEX [IX_Devices_Status] ON [Devices] ([Status]);

CREATE UNIQUE INDEX [IX_Devices_TenantId_DeviceId] ON [Devices] ([TenantId], [DeviceId]);

CREATE INDEX [IX_TelemetryEvents_DeviceId] ON [TelemetryEvents] ([DeviceId]);

CREATE INDEX [IX_TelemetryEvents_EventType] ON [TelemetryEvents] ([EventType]);

CREATE INDEX [IX_TelemetryEvents_ReceivedAt] ON [TelemetryEvents] ([ReceivedAt]);

CREATE INDEX [IX_TelemetryEvents_TenantId] ON [TelemetryEvents] ([TenantId]);

CREATE UNIQUE INDEX [IX_Tenants_ApiKey] ON [Tenants] ([ApiKey]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260211022026_InitialCreate', N'9.0.0');

COMMIT;
GO

