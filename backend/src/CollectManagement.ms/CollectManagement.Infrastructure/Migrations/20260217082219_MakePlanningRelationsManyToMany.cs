using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollectManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakePlanningRelationsManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_PlanningDevice_DeviceId",
                table: "PlanningDevice",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningEmployee_EmployeeId",
                table: "PlanningEmployee",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningShift_ShiftId",
                table: "PlanningShift",
                column: "ShiftId");

            migrationBuilder.Sql(
                @"INSERT INTO PlanningDevice (PlanningId, DeviceId, InsererPar, DateInsertion, ModifierPar, DateModification)
                  SELECT PlanningId, DeviceId, InsererPar, DateInsertion, ModifierPar, DateModification
                  FROM Planning");

            migrationBuilder.Sql(
                @"INSERT INTO PlanningEmployee (PlanningId, EmployeeId, InsererPar, DateInsertion, ModifierPar, DateModification)
                  SELECT PlanningId, EmployeeId, InsererPar, DateInsertion, ModifierPar, DateModification
                  FROM Planning");

            migrationBuilder.Sql(
                @"INSERT INTO PlanningShift (PlanningId, ShiftId, InsererPar, DateInsertion, ModifierPar, DateModification)
                  SELECT PlanningId, ShiftId, InsererPar, DateInsertion, ModifierPar, DateModification
                  FROM Planning");

            migrationBuilder.DropForeignKey(
                name: "FK_Planning_Device_DeviceId",
                table: "Planning");

            migrationBuilder.DropForeignKey(
                name: "FK_Planning_Employee_EmployeeId",
                table: "Planning");

            migrationBuilder.DropForeignKey(
                name: "FK_Planning_Shift_ShiftId",
                table: "Planning");

            migrationBuilder.DropIndex(
                name: "IX_Planning_DeviceId",
                table: "Planning");

            migrationBuilder.DropIndex(
                name: "IX_Planning_EmployeeId",
                table: "Planning");

            migrationBuilder.DropIndex(
                name: "IX_Planning_ShiftId",
                table: "Planning");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "Planning");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "Planning");

            migrationBuilder.DropColumn(
                name: "ShiftId",
                table: "Planning");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanningDevice");

            migrationBuilder.DropTable(
                name: "PlanningEmployee");

            migrationBuilder.DropTable(
                name: "PlanningShift");

            migrationBuilder.AddColumn<Guid>(
                name: "DeviceId",
                table: "Planning",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeId",
                table: "Planning",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ShiftId",
                table: "Planning",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Planning_DeviceId",
                table: "Planning",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Planning_EmployeeId",
                table: "Planning",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Planning_ShiftId",
                table: "Planning",
                column: "ShiftId");

            migrationBuilder.AddForeignKey(
                name: "FK_Planning_Device_DeviceId",
                table: "Planning",
                column: "DeviceId",
                principalTable: "Device",
                principalColumn: "DeviceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Planning_Employee_EmployeeId",
                table: "Planning",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Planning_Shift_ShiftId",
                table: "Planning",
                column: "ShiftId",
                principalTable: "Shift",
                principalColumn: "ShiftId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
