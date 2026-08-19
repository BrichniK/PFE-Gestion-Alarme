BEGIN TRANSACTION;
GO

ALTER TABLE [Alerte] DROP CONSTRAINT [FK_Alerte_Device_DispositifId];
GO

ALTER TABLE [Alerte] DROP CONSTRAINT [FK_Alerte_Type_TypeId];
GO

ALTER TABLE [Maintenance] DROP CONSTRAINT [FK_Maintenance_Device_DeviceId];
GO

ALTER TABLE [MaintenanceCaptureHistory] DROP CONSTRAINT [FK_MaintenanceCaptureHistory_Device_DeviceId];
GO

ALTER TABLE [PlanningDevice] DROP CONSTRAINT [FK_PlanningDevice_Device_DeviceId];
GO

ALTER TABLE [SMSDevice] DROP CONSTRAINT [FK_SMSDevice_Device_DeviceId];
GO

ALTER TABLE [Type] DROP CONSTRAINT [PK_Type];
GO

ALTER TABLE [Device] DROP CONSTRAINT [PK_Device];
GO

ALTER TABLE [Alerte] DROP CONSTRAINT [PK_Alerte];
GO

EXEC sp_rename N'[Type]', N'Types';
GO

EXEC sp_rename N'[Device]', N'Devices';
GO

EXEC sp_rename N'[Alerte]', N'Alertes';
GO

EXEC sp_rename N'[Alertes].[IX_Alerte_TypeId]', N'IX_Alertes_TypeId', N'INDEX';
GO

EXEC sp_rename N'[Alertes].[IX_Alerte_DispositifId]', N'IX_Alertes_DispositifId', N'INDEX';
GO

ALTER TABLE [Types] ADD CONSTRAINT [PK_Types] PRIMARY KEY ([TypeId]);
GO

ALTER TABLE [Devices] ADD CONSTRAINT [PK_Devices] PRIMARY KEY ([DeviceId]);
GO

ALTER TABLE [Alertes] ADD CONSTRAINT [PK_Alertes] PRIMARY KEY ([AlerteId]);
GO

CREATE TABLE [SensorMeasurements] (
    [SensorMeasurementId] uniqueidentifier NOT NULL,
    [DeviceId] uniqueidentifier NOT NULL,
    [SensorCode] nvarchar(200) NOT NULL,
    [MeasuredAt] datetime2 NOT NULL,
    [Temperature] float NULL,
    [Vibration] float NULL,
    [Pressure] float NULL,
    [Humidity] float NULL,
    [IsFailure] bit NOT NULL DEFAULT CAST(0 AS bit),
    [InsererPar] nvarchar(max) NULL,
    [DateInsertion] datetime2 NULL,
    [ModifierPar] nvarchar(max) NULL,
    [DateModification] datetime2 NULL,
    CONSTRAINT [PK_SensorMeasurements] PRIMARY KEY ([SensorMeasurementId])
);
GO

CREATE INDEX [IX_SensorMeasurements_DeviceId_MeasuredAt] ON [SensorMeasurements] ([DeviceId], [MeasuredAt]);
GO

ALTER TABLE [Alertes] ADD CONSTRAINT [FK_Alertes_Devices_DispositifId] FOREIGN KEY ([DispositifId]) REFERENCES [Devices] ([DeviceId]) ON DELETE NO ACTION;
GO

ALTER TABLE [Alertes] ADD CONSTRAINT [FK_Alertes_Types_TypeId] FOREIGN KEY ([TypeId]) REFERENCES [Types] ([TypeId]) ON DELETE NO ACTION;
GO

ALTER TABLE [Maintenance] ADD CONSTRAINT [FK_Maintenance_Devices_DeviceId] FOREIGN KEY ([DeviceId]) REFERENCES [Devices] ([DeviceId]) ON DELETE NO ACTION;
GO

ALTER TABLE [MaintenanceCaptureHistory] ADD CONSTRAINT [FK_MaintenanceCaptureHistory_Devices_DeviceId] FOREIGN KEY ([DeviceId]) REFERENCES [Devices] ([DeviceId]) ON DELETE NO ACTION;
GO

ALTER TABLE [PlanningDevice] ADD CONSTRAINT [FK_PlanningDevice_Devices_DeviceId] FOREIGN KEY ([DeviceId]) REFERENCES [Devices] ([DeviceId]) ON DELETE NO ACTION;
GO

ALTER TABLE [SMSDevice] ADD CONSTRAINT [FK_SMSDevice_Devices_DeviceId] FOREIGN KEY ([DeviceId]) REFERENCES [Devices] ([DeviceId]) ON DELETE NO ACTION;
GO

INSERT INTO [F3SManagement].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260813043037_AddSensorMeasurements', N'8.0.6');
GO

COMMIT;
GO

