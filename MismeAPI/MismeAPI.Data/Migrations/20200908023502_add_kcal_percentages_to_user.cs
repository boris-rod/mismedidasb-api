using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class add_kcal_percentages_to_user : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BreakFastKCalPercentage",
                table: "user",
                nullable: false,
                defaultValue: 20);

            migrationBuilder.AddColumn<int>(
                name: "DinnerKCalPercentage",
                table: "user",
                nullable: false,
                defaultValue: 25);

            migrationBuilder.AddColumn<int>(
                name: "LunchKCalPercentage",
                table: "user",
                nullable: false,
                defaultValue: 35);

            migrationBuilder.AddColumn<int>(
                name: "Snack1KCalPercentage",
                table: "user",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<int>(
                name: "Snack2KCalPercentage",
                table: "user",
                nullable: false,
                defaultValue: 10);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BreakFastKCalPercentage",
                table: "user");

            migrationBuilder.DropColumn(
                name: "DinnerKCalPercentage",
                table: "user");

            migrationBuilder.DropColumn(
                name: "LunchKCalPercentage",
                table: "user");

            migrationBuilder.DropColumn(
                name: "Snack1KCalPercentage",
                table: "user");

            migrationBuilder.DropColumn(
                name: "Snack2KCalPercentage",
                table: "user");
        }
    }
}
