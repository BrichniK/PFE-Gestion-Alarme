using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollectManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSensorMeasurements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerte_Device_DispositifId",
                table: "Alerte");

            migrationBuilder.DropForeignKey(
                name: "FK_Alerte_Type_TypeId",
                table: "Alerte");

            migrationBuilder.DropForeignKey(
                name: "FK_Maintenance_Device_DeviceId",
                table: "Maintenance");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceCaptureHistory_Device_DeviceId",
                table: "MaintenanceCaptureHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanningDevice_Device_DeviceId",
                table: "PlanningDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_SMSDevice_Device_DeviceId",
                table: "SMSDevice");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Type",
                table: "Type");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Device",
                table: "Device");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Alerte",
                table: "Alerte");

            migrationBuilder.RenameTable(
                name: "Type",
                newName: "Types");

            migrationBuilder.RenameTable(
                name: "Device",
                newName: "Devices");

            migrationBuilder.RenameTable(
                name: "Alerte",
                newName: "Alertes");

            migrationBuilder.RenameIndex(
                name: "IX_Alerte_TypeId",
                table: "Alertes",
                newName: "IX_Alertes_TypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Alerte_DispositifId",
                table: "Alertes",
                newName: "IX_Alertes_DispositifId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Types",
                table: "Types",
                column: "TypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Devices",
                table: "Devices",
                column: "DeviceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Alertes",
                table: "Alertes",
                column: "AlerteId");

            migrationBuilder.CreateTable(
                name: "SensorMeasurements",
                columns: table => new
                {
                    SensorMeasurementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SensorCode = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    MeasuredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Temperature = table.Column<double>(type: "float", nullable: true),
                    Vibration = table.Column<double>(type: "float", nullable: true),
                    Pressure = table.Column<double>(type: "float", nullable: true),
                    Humidity = table.Column<double>(type: "float", nullable: true),
                    IsFailure = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorMeasurements", x => x.SensorMeasurementId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SensorMeasurements_DeviceId_MeasuredAt",
                table: "SensorMeasurements",
                columns: new[] { "DeviceId", "MeasuredAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_Alertes_Devices_DispositifId",
                table: "Alertes",
                column: "DispositifId",
                principalTable: "Devices",
                principalColumn: "DeviceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Alertes_Types_TypeId",
                table: "Alertes",
                column: "TypeId",
                principalTable: "Types",
                principalColumn: "TypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Maintenance_Devices_DeviceId",
                table: "Maintenance",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "DeviceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceCaptureHistory_Devices_DeviceId",
                table: "MaintenanceCaptureHistory",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "DeviceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningDevice_Devices_DeviceId",
                table: "PlanningDevice",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "DeviceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SMSDevice_Devices_DeviceId",
                table: "SMSDevice",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "DeviceId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alertes_Devices_DispositifId",
                table: "Alertes");

            migrationBuilder.DropForeignKey(
                name: "FK_Alertes_Types_TypeId",
                table: "Alertes");

            migrationBuilder.DropForeignKey(
                name: "FK_Maintenance_Devices_DeviceId",
                table: "Maintenance");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceCaptureHistory_Devices_DeviceId",
                table: "MaintenanceCaptureHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanningDevice_Devices_DeviceId",
                table: "PlanningDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_SMSDevice_Devices_DeviceId",
                table: "SMSDevice");

            migrationBuilder.DropTable(
                name: "SensorMeasurements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Types",
                table: "Types");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Devices",
                table: "Devices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Alertes",
                table: "Alertes");

            migrationBuilder.RenameTable(
                name: "Types",
                newName: "Type");

            migrationBuilder.RenameTable(
                name: "Devices",
                newName: "Device");

            migrationBuilder.RenameTable(
                name: "Alertes",
                newName: "Alerte");

            migrationBuilder.RenameIndex(
                name: "IX_Alertes_TypeId",
                table: "Alerte",
                newName: "IX_Alerte_TypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Alertes_DispositifId",
                table: "Alerte",
                newName: "IX_Alerte_DispositifId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Type",
                table: "Type",
                column: "TypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Device",
                table: "Device",
                column: "DeviceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Alerte",
                table: "Alerte",
                column: "AlerteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerte_Device_DispositifId",
                table: "Alerte",
                column: "DispositifId",
                principalTable: "Device",
                principalColumn: "DeviceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Alerte_Type_TypeId",
                table: "Alerte",
                column: "TypeId",
                principalTable: "Type",
                principalColumn: "TypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Maintenance_Device_DeviceId",
                table: "Maintenance",
                column: "DeviceId",
                principalTable: "Device",
                principalColumn: "DeviceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceCaptureHistory_Device_DeviceId",
                table: "MaintenanceCaptureHistory",
                column: "DeviceId",
                principalTable: "Device",
                principalColumn: "DeviceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningDevice_Device_DeviceId",
                table: "PlanningDevice",
                column: "DeviceId",
                principalTable: "Device",
                principalColumn: "DeviceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SMSDevice_Device_DeviceId",
                table: "SMSDevice",
                column: "DeviceId",
                principalTable: "Device",
                principalColumn: "DeviceId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
