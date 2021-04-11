using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class add_retry_to_scheduled_emails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExceptionMessage",
                table: "scheduledemail",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "scheduledemail",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExceptionMessage",
                table: "scheduledemail");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "scheduledemail");
        }
    }
}
