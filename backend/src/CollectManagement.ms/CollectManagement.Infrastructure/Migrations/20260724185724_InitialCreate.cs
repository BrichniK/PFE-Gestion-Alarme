using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollectManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfigurationGenerale",
                columns: table => new
                {
                    ConfigurationGeneraleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EcraserEmployeMaintenance = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AccepterSeulementEmployesPlanifies = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DiagnostiqueObligatoire = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    MonitoringPourcentageSurSommeDurees = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CoefficientGaugeD1 = table.Column<double>(type: "float", nullable: false, defaultValue: 1.0),
                    CoefficientGaugeD2 = table.Column<double>(type: "float", nullable: false, defaultValue: 1.0),
                    CoefficientGaugeD3 = table.Column<double>(type: "float", nullable: false, defaultValue: 1.0),
                    CoefficientGaugeD4 = table.Column<double>(type: "float", nullable: false, defaultValue: 1.0),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationGenerale", x => x.ConfigurationGeneraleId);
                });

            migrationBuilder.CreateTable(
                name: "Device",
                columns: table => new
                {
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceName = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    Matricule = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    NombreCapteur = table.Column<int>(type: "int", nullable: false),
                    IsOnline = table.Column<bool>(type: "bit", nullable: false),
                    LastSeen = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Device", x => x.DeviceId);
                });

            migrationBuilder.CreateTable(
                name: "Employee",
                columns: table => new
                {
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nom = table.Column<string>(type: "varchar(100)", nullable: false),
                    Prenom = table.Column<string>(type: "varchar(100)", nullable: false),
                    Phone = table.Column<int>(type: "int", nullable: false),
                    Rfid = table.Column<string>(type: "varchar(100)", nullable: false),
                    Email = table.Column<string>(type: "varchar(255)", nullable: true),
                    LogoPath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employee", x => x.EmployeeId);
                });

            migrationBuilder.CreateTable(
                name: "Groupe",
                columns: table => new
                {
                    GroupeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nom = table.Column<string>(type: "varchar(200)", nullable: false),
                    Color = table.Column<string>(type: "varchar(20)", nullable: true),
                    EmployeeIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groupe", x => x.GroupeId);
                });

            migrationBuilder.CreateTable(
                name: "JourFerie",
                columns: table => new
                {
                    JourFerieId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JourFerie", x => x.JourFerieId);
                });

            migrationBuilder.CreateTable(
                name: "Planning",
                columns: table => new
                {
                    PlanningId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planning", x => x.PlanningId);
                });

            migrationBuilder.CreateTable(
                name: "RoleUtilisateurs",
                columns: table => new
                {
                    RoleUtilisateurId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LibelleRoleUtilisateur = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleUtilisateurs", x => x.RoleUtilisateurId);
                });

            migrationBuilder.CreateTable(
                name: "Shift",
                columns: table => new
                {
                    ShiftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shift", x => x.ShiftId);
                });

            migrationBuilder.CreateTable(
                name: "SMS",
                columns: table => new
                {
                    SMSId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomPrenom = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SMS", x => x.SMSId);
                });

            migrationBuilder.CreateTable(
                name: "SMSConfiguration",
                columns: table => new
                {
                    SMSConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApiUrl = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    NombreAlerte = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Delai = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SmsOnAlerte = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SmsOnBadgeT3 = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SmsOnBadgeT4 = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SmsOnBadgeT5 = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SmsOnTraitement = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SMSConfiguration", x => x.SMSConfigurationId);
                });

            migrationBuilder.CreateTable(
                name: "Societes",
                columns: table => new
                {
                    SocieteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LogoPath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Nom = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatriculeFiscal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Rne = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Capital = table.Column<decimal>(type: "decimal(18,3)", nullable: false, defaultValue: 0m),
                    DateOverture = table.Column<DateTime>(type: "date", nullable: false),
                    Telephone1 = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    Telephone2 = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    Fax1 = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    Fax2 = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Adresse = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CodeSociete = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Societes", x => x.SocieteId);
                });

            migrationBuilder.CreateTable(
                name: "Type",
                columns: table => new
                {
                    TypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    DureeNominal = table.Column<int>(type: "int", nullable: true),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Type", x => x.TypeId);
                });

            migrationBuilder.CreateTable(
                name: "Maintenance",
                columns: table => new
                {
                    MaintenanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    T1Alerte = table.Column<DateTime>(type: "datetime2", nullable: true),
                    T2Assignment = table.Column<DateTime>(type: "datetime2", nullable: true),
                    T3Arrival = table.Column<DateTime>(type: "datetime2", nullable: true),
                    T4Completion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    T5Confirmation = table.Column<DateTime>(type: "datetime2", nullable: true),
                    T6NextAlert = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "varchar(500)", nullable: false),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maintenance", x => x.MaintenanceId);
                    table.ForeignKey(
                        name: "FK_Maintenance_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Device",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Maintenance_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlanningDevice",
                columns: table => new
                {
                    PlanningId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningDevice", x => new { x.PlanningId, x.DeviceId });
                    table.ForeignKey(
                        name: "FK_PlanningDevice_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Device",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanningDevice_Planning_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Planning",
                        principalColumn: "PlanningId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanningEmployee",
                columns: table => new
                {
                    PlanningId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningEmployee", x => new { x.PlanningId, x.EmployeeId });
                    table.ForeignKey(
                        name: "FK_PlanningEmployee_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanningEmployee_Planning_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Planning",
                        principalColumn: "PlanningId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanningGroupe",
                columns: table => new
                {
                    PlanningId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningGroupe", x => new { x.PlanningId, x.GroupeId });
                    table.ForeignKey(
                        name: "FK_PlanningGroupe_Groupe_GroupeId",
                        column: x => x.GroupeId,
                        principalTable: "Groupe",
                        principalColumn: "GroupeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanningGroupe_Planning_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Planning",
                        principalColumn: "PlanningId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Navigation",
                columns: table => new
                {
                    NavigationId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    RoleUtilisateurId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Actions = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Navigation", x => new { x.NavigationId, x.RoleUtilisateurId });
                    table.ForeignKey(
                        name: "FK_Navigation_RoleUtilisateurs_RoleUtilisateurId",
                        column: x => x.RoleUtilisateurId,
                        principalTable: "RoleUtilisateurs",
                        principalColumn: "RoleUtilisateurId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanningShift",
                columns: table => new
                {
                    PlanningId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningShift", x => new { x.PlanningId, x.ShiftId });
                    table.ForeignKey(
                        name: "FK_PlanningShift_Planning_PlanningId",
                        column: x => x.PlanningId,
                        principalTable: "Planning",
                        principalColumn: "PlanningId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanningShift_Shift_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shift",
                        principalColumn: "ShiftId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SMSDevice",
                columns: table => new
                {
                    SMSId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SMSDevice", x => new { x.SMSId, x.DeviceId });
                    table.ForeignKey(
                        name: "FK_SMSDevice_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Device",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SMSDevice_SMS_SMSId",
                        column: x => x.SMSId,
                        principalTable: "SMS",
                        principalColumn: "SMSId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    UtilisateurId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomUtilisateur = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoleUtilisateurId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SocieteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.UtilisateurId);
                    table.ForeignKey(
                        name: "FK_Utilisateurs_RoleUtilisateurs_RoleUtilisateurId",
                        column: x => x.RoleUtilisateurId,
                        principalTable: "RoleUtilisateurs",
                        principalColumn: "RoleUtilisateurId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Utilisateurs_Societes_SocieteId",
                        column: x => x.SocieteId,
                        principalTable: "Societes",
                        principalColumn: "SocieteId");
                });

            migrationBuilder.CreateTable(
                name: "Alerte",
                columns: table => new
                {
                    AlerteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DispositifId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Traiter = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerte", x => x.AlerteId);
                    table.ForeignKey(
                        name: "FK_Alerte_Device_DispositifId",
                        column: x => x.DispositifId,
                        principalTable: "Device",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Alerte_Type_TypeId",
                        column: x => x.TypeId,
                        principalTable: "Type",
                        principalColumn: "TypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceCaptureHistory",
                columns: table => new
                {
                    MaintenanceCaptureHistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaintenanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagId = table.Column<string>(type: "varchar(100)", nullable: false),
                    Step = table.Column<string>(type: "varchar(10)", nullable: false),
                    Status = table.Column<string>(type: "varchar(30)", nullable: false),
                    CapturedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceCaptureHistory", x => x.MaintenanceCaptureHistoryId);
                    table.ForeignKey(
                        name: "FK_MaintenanceCaptureHistory_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Device",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenanceCaptureHistory_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenanceCaptureHistory_Maintenance_MaintenanceId",
                        column: x => x.MaintenanceId,
                        principalTable: "Maintenance",
                        principalColumn: "MaintenanceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NavigationSection",
                columns: table => new
                {
                    SectionId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    Actions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NavigationId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    NavigationRoleUtilisateurId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavigationSection", x => x.SectionId);
                    table.ForeignKey(
                        name: "FK_NavigationSection_Navigation_NavigationId_NavigationRoleUtilisateurId",
                        columns: x => new { x.NavigationId, x.NavigationRoleUtilisateurId },
                        principalTable: "Navigation",
                        principalColumns: new[] { "NavigationId", "RoleUtilisateurId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alerte_DispositifId",
                table: "Alerte",
                column: "DispositifId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerte_TypeId",
                table: "Alerte",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_JourFerie_Date",
                table: "JourFerie",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Maintenance_DeviceId",
                table: "Maintenance",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Maintenance_EmployeeId",
                table: "Maintenance",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceCaptureHistory_CapturedAt",
                table: "MaintenanceCaptureHistory",
                column: "CapturedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceCaptureHistory_DeviceId",
                table: "MaintenanceCaptureHistory",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceCaptureHistory_EmployeeId",
                table: "MaintenanceCaptureHistory",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceCaptureHistory_MaintenanceId",
                table: "MaintenanceCaptureHistory",
                column: "MaintenanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Navigation_RoleUtilisateurId",
                table: "Navigation",
                column: "RoleUtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_NavigationSection_NavigationId_NavigationRoleUtilisateurId",
                table: "NavigationSection",
                columns: new[] { "NavigationId", "NavigationRoleUtilisateurId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlanningDevice_DeviceId",
                table: "PlanningDevice",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningEmployee_EmployeeId",
                table: "PlanningEmployee",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningGroupe_GroupeId",
                table: "PlanningGroupe",
                column: "GroupeId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningShift_ShiftId",
                table: "PlanningShift",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_SMSDevice_DeviceId",
                table: "SMSDevice",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_Email",
                table: "Utilisateurs",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_NomUtilisateur",
                table: "Utilisateurs",
                column: "NomUtilisateur",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_RoleUtilisateurId",
                table: "Utilisateurs",
                column: "RoleUtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_SocieteId",
                table: "Utilisateurs",
                column: "SocieteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alerte");

            migrationBuilder.DropTable(
                name: "ConfigurationGenerale");

            migrationBuilder.DropTable(
                name: "JourFerie");

            migrationBuilder.DropTable(
                name: "MaintenanceCaptureHistory");

            migrationBuilder.DropTable(
                name: "NavigationSection");

            migrationBuilder.DropTable(
                name: "PlanningDevice");

            migrationBuilder.DropTable(
                name: "PlanningEmployee");

            migrationBuilder.DropTable(
                name: "PlanningGroupe");

            migrationBuilder.DropTable(
                name: "PlanningShift");

            migrationBuilder.DropTable(
                name: "SMSConfiguration");

            migrationBuilder.DropTable(
                name: "SMSDevice");

            migrationBuilder.DropTable(
                name: "Utilisateurs");

            migrationBuilder.DropTable(
                name: "Type");

            migrationBuilder.DropTable(
                name: "Maintenance");

            migrationBuilder.DropTable(
                name: "Navigation");

            migrationBuilder.DropTable(
                name: "Groupe");

            migrationBuilder.DropTable(
                name: "Planning");

            migrationBuilder.DropTable(
                name: "Shift");

            migrationBuilder.DropTable(
                name: "SMS");

            migrationBuilder.DropTable(
                name: "Societes");

            migrationBuilder.DropTable(
                name: "Device");

            migrationBuilder.DropTable(
                name: "Employee");

            migrationBuilder.DropTable(
                name: "RoleUtilisateurs");
        }
    }
}
