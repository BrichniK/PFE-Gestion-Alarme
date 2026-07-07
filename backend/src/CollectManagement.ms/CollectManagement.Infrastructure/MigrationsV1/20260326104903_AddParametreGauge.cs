using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollectManagement.Infrastructure.MigrationsV1
{
    /// <inheritdoc />
    public partial class AddParametreGauge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MonitoringPourcentageSurSommeDurees",
                table: "ConfigurationGenerale",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonitoringPourcentageSurSommeDurees",
                table: "ConfigurationGenerale");
        }
    }
}
