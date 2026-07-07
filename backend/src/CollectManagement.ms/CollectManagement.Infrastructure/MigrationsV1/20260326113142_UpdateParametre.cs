using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollectManagement.Infrastructure.MigrationsV1
{
    /// <inheritdoc />
    public partial class UpdateParametre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CoefficientGaugeD1",
                table: "ConfigurationGenerale",
                type: "float",
                nullable: false,
                defaultValue: 1.0);

            migrationBuilder.AddColumn<double>(
                name: "CoefficientGaugeD2",
                table: "ConfigurationGenerale",
                type: "float",
                nullable: false,
                defaultValue: 1.0);

            migrationBuilder.AddColumn<double>(
                name: "CoefficientGaugeD3",
                table: "ConfigurationGenerale",
                type: "float",
                nullable: false,
                defaultValue: 1.0);

            migrationBuilder.AddColumn<double>(
                name: "CoefficientGaugeD4",
                table: "ConfigurationGenerale",
                type: "float",
                nullable: false,
                defaultValue: 1.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoefficientGaugeD1",
                table: "ConfigurationGenerale");

            migrationBuilder.DropColumn(
                name: "CoefficientGaugeD2",
                table: "ConfigurationGenerale");

            migrationBuilder.DropColumn(
                name: "CoefficientGaugeD3",
                table: "ConfigurationGenerale");

            migrationBuilder.DropColumn(
                name: "CoefficientGaugeD4",
                table: "ConfigurationGenerale");
        }
    }
}
