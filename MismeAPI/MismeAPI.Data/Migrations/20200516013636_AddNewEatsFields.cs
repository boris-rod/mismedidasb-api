using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class AddNewEatsFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Vitamins",
                table: "dish");

            migrationBuilder.AddColumn<double>(
                name: "Calcium",
                table: "dish",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FolicAcid",
                table: "dish",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Iron",
                table: "dish",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Magnesium",
                table: "dish",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Niacin",
                table: "dish",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Phosphorus",
                table: "dish",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Potassium",
                table: "dish",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Ribofla",
                table: "dish",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Sodium",
                table: "dish",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Thiamine",
                table: "dish",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "VitaminA",
                table: "dish",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "VitaminB12",
                table: "dish",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "VitaminB6",
                table: "dish",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "VitaminC",
                table: "dish",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Zinc",
                table: "dish",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Calcium",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "FolicAcid",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "Iron",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "Magnesium",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "Niacin",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "Phosphorus",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "Potassium",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "Ribofla",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "Sodium",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "Thiamine",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "VitaminA",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "VitaminB12",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "VitaminB6",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "VitaminC",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "Zinc",
                table: "dish");

            migrationBuilder.AddColumn<double>(
                name: "Vitamins",
                table: "dish",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
