using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class AddDishColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Cholesterol",
                table: "dish",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "IsCaloric",
                table: "dish",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFruitAndVegetables",
                table: "dish",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsProteic",
                table: "dish",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Vitamins",
                table: "dish",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cholesterol",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "IsCaloric",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "IsFruitAndVegetables",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "IsProteic",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "Vitamins",
                table: "dish");
        }
    }
}
