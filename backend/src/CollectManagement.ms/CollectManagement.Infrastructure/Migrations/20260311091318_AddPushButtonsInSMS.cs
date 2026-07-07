using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollectManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPushButtonsInSMS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SmsOnAlerte",
                table: "SMSConfiguration",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "SmsOnBadge",
                table: "SMSConfiguration",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "SmsOnTraitement",
                table: "SMSConfiguration",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmsOnAlerte",
                table: "SMSConfiguration");

            migrationBuilder.DropColumn(
                name: "SmsOnBadge",
                table: "SMSConfiguration");

            migrationBuilder.DropColumn(
                name: "SmsOnTraitement",
                table: "SMSConfiguration");
        }
    }
}
