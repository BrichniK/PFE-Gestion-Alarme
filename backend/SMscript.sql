BEGIN TRANSACTION;
GO

CREATE TABLE [SMS] (
    [SMSId] uniqueidentifier NOT NULL,
    [NomPrenom] nvarchar(200) NOT NULL,
    [PhoneNumber] nvarchar(20) NOT NULL,
    [InsererPar] nvarchar(max) NULL,
    [DateInsertion] datetime2 NULL,
    [ModifierPar] nvarchar(max) NULL,
    [DateModification] datetime2 NULL,
    CONSTRAINT [PK_SMS] PRIMARY KEY ([SMSId])
);
GO

CREATE TABLE [SMSDevice] (
    [SMSId] uniqueidentifier NOT NULL,
    [DeviceId] uniqueidentifier NOT NULL,
    [InsererPar] nvarchar(max) NULL,
    [DateInsertion] datetime2 NULL,
    [ModifierPar] nvarchar(max) NULL,
    [DateModification] datetime2 NULL,
    CONSTRAINT [PK_SMSDevice] PRIMARY KEY ([SMSId], [DeviceId]),
    CONSTRAINT [FK_SMSDevice_Device_DeviceId] FOREIGN KEY ([DeviceId]) REFERENCES [Device] ([DeviceId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SMSDevice_SMS_SMSId] FOREIGN KEY ([SMSId]) REFERENCES [SMS] ([SMSId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_SMSDevice_DeviceId] ON [SMSDevice] ([DeviceId]);
GO

INSERT INTO [F3SManagement].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260220091658_AddSMSToProject', N'8.0.6');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [SMSConfiguration] (
    [SMSConfigurationId] uniqueidentifier NOT NULL,
    [ApiUrl] nvarchar(500) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit),
    [InsererPar] nvarchar(max) NULL,
    [DateInsertion] datetime2 NULL,
    [ModifierPar] nvarchar(max) NULL,
    [DateModification] datetime2 NULL,
    CONSTRAINT [PK_SMSConfiguration] PRIMARY KEY ([SMSConfigurationId])
);
GO

INSERT INTO [F3SManagement].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260302073716_AddSMSConfiguration', N'8.0.6');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Employee] ADD [Email] varchar(255) NULL;
GO

INSERT INTO [F3SManagement].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260302090746_AddEmailDansEmployee', N'8.0.6');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Maintenance] ADD [T5Confirmation] datetime2 NULL;
GO

INSERT INTO [F3SManagement].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260302110742_AddT5ToMaintenance', N'8.0.6');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Maintenance] ADD [T6NextAlert] datetime2 NULL;
GO

INSERT INTO [F3SManagement].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260303103230_AddT6InTableMaintenance', N'8.0.6');
GO

COMMIT;
GO

