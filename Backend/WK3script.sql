BEGIN TRANSACTION;
GO

CREATE TABLE [Groupe] (
    [GroupeId] uniqueidentifier NOT NULL,
    [Nom] varchar(200) NOT NULL,
    [EmployeeIds] nvarchar(max) NOT NULL,
    [InsererPar] nvarchar(max) NULL,
    [DateInsertion] datetime2 NULL,
    [ModifierPar] nvarchar(max) NULL,
    [DateModification] datetime2 NULL,
    CONSTRAINT [PK_Groupe] PRIMARY KEY ([GroupeId])
);
GO

INSERT INTO [F3SManagement].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260309092415_AddGroupes', N'8.0.6');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [PlanningGroupe] (
    [PlanningId] uniqueidentifier NOT NULL,
    [GroupeId] uniqueidentifier NOT NULL,
    [InsererPar] nvarchar(max) NULL,
    [DateInsertion] datetime2 NULL,
    [ModifierPar] nvarchar(max) NULL,
    [DateModification] datetime2 NULL,
    CONSTRAINT [PK_PlanningGroupe] PRIMARY KEY ([PlanningId], [GroupeId]),
    CONSTRAINT [FK_PlanningGroupe_Groupe_GroupeId] FOREIGN KEY ([GroupeId]) REFERENCES [Groupe] ([GroupeId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PlanningGroupe_Planning_PlanningId] FOREIGN KEY ([PlanningId]) REFERENCES [Planning] ([PlanningId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_PlanningGroupe_GroupeId] ON [PlanningGroupe] ([GroupeId]);
GO

INSERT INTO [F3SManagement].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260309100122_AddGroupeToPlanning', N'8.0.6');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Groupe] ADD [Color] varchar(20) NULL;
GO

INSERT INTO [F3SManagement].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260309102414_AddColorToGroupe', N'8.0.6');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [SMSConfiguration] ADD [Delai] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [SMSConfiguration] ADD [NombreAlerte] int NOT NULL DEFAULT 1;
GO

INSERT INTO [F3SManagement].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260310083251_AddDelaiAndNombreAlerte', N'8.0.6');
GO

COMMIT;
GO

