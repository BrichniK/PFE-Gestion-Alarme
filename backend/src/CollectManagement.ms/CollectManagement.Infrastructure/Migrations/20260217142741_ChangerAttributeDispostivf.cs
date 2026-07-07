using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollectManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangerAttributeDispostivf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Device",
                table: "Alerte");

            migrationBuilder.AddColumn<Guid>(
                name: "DispositifId",
                table: "Alerte",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alerte_DispositifId",
                table: "Alerte",
                column: "DispositifId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerte_Device_DispositifId",
                table: "Alerte",
                column: "DispositifId",
                principalTable: "Device",
                principalColumn: "DeviceId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerte_Device_DispositifId",
                table: "Alerte");

            migrationBuilder.DropIndex(
                name: "IX_Alerte_DispositifId",
                table: "Alerte");

            migrationBuilder.DropColumn(
                name: "DispositifId",
                table: "Alerte");

            migrationBuilder.AddColumn<string>(
                name: "Device",
                table: "Alerte",
                type: "nvarchar(200)",
                nullable: true);
        }
    }
}
