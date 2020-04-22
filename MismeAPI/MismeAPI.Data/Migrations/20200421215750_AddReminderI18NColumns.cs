using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class AddReminderI18NColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BodyEN",
                table: "reminder",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BodyIT",
                table: "reminder",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleEN",
                table: "reminder",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleIT",
                table: "reminder",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BodyEN",
                table: "reminder");

            migrationBuilder.DropColumn(
                name: "BodyIT",
                table: "reminder");

            migrationBuilder.DropColumn(
                name: "TitleEN",
                table: "reminder");

            migrationBuilder.DropColumn(
                name: "TitleIT",
                table: "reminder");
        }
    }
}
