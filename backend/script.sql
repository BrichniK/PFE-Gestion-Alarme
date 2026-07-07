BEGIN TRANSACTION;
GO

ALTER TABLE [Type] ADD [DureeNominal] int NULL;
GO

INSERT INTO [F3SManagement].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260218133507_AddConsigneToTypeAlert', N'8.0.6');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF COL_LENGTH('Device', 'GroupName') IS NULL
BEGIN
    ALTER TABLE [Device] ADD [GroupName] nvarchar(150) NULL;
END
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [MaintenanceCaptureHistory] (
    [MaintenanceCaptureHistoryId] uniqueidentifier NOT NULL,
    [MaintenanceId] uniqueidentifier NOT NULL,
    [DeviceId] uniqueidentifier NOT NULL,
    [EmployeeId] uniqueidentifier NOT NULL,
    [TagId] varchar(100) NOT NULL,
    [Step] varchar(10) NOT NULL,
    [Status] varchar(30) NOT NULL,
    [CapturedAt] datetime2 NOT NULL,
    [InsererPar] nvarchar(max) NULL,
    [DateInsertion] datetime2 NULL,
    [ModifierPar] nvarchar(max) NULL,
    [DateModification] datetime2 NULL,
    CONSTRAINT [PK_MaintenanceCaptureHistory] PRIMARY KEY ([MaintenanceCaptureHistoryId]),
    CONSTRAINT [FK_MaintenanceCaptureHistory_Device_DeviceId] FOREIGN KEY ([DeviceId]) REFERENCES [Device] ([DeviceId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MaintenanceCaptureHistory_Employee_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employee] ([EmployeeId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MaintenanceCaptureHistory_Maintenance_MaintenanceId] FOREIGN KEY ([MaintenanceId]) REFERENCES [Maintenance] ([MaintenanceId]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_MaintenanceCaptureHistory_CapturedAt] ON [MaintenanceCaptureHistory] ([CapturedAt]);
GO

CREATE INDEX [IX_MaintenanceCaptureHistory_DeviceId] ON [MaintenanceCaptureHistory] ([DeviceId]);
GO

CREATE INDEX [IX_MaintenanceCaptureHistory_EmployeeId] ON [MaintenanceCaptureHistory] ([EmployeeId]);
GO

CREATE INDEX [IX_MaintenanceCaptureHistory_MaintenanceId] ON [MaintenanceCaptureHistory] ([MaintenanceId]);
GO

INSERT INTO [F3SManagement].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260218141610_AddMaintenanceCaptureHistory', N'8.0.6');
GO

COMMIT;
GO
