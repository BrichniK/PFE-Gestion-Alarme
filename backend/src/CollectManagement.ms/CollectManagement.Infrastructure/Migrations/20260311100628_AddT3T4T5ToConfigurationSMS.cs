using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollectManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddT3T4T5ToConfigurationSMS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SmsOnBadge",
                table: "SMSConfiguration",
                newName: "SmsOnBadgeT5");

            migrationBuilder.AddColumn<bool>(
                name: "SmsOnBadgeT3",
                table: "SMSConfiguration",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "SmsOnBadgeT4",
                table: "SMSConfiguration",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmsOnBadgeT3",
                table: "SMSConfiguration");

            migrationBuilder.DropColumn(
                name: "SmsOnBadgeT4",
                table: "SMSConfiguration");

            migrationBuilder.RenameColumn(
                name: "SmsOnBadgeT5",
                table: "SMSConfiguration",
                newName: "SmsOnBadge");
        }
    }
}
