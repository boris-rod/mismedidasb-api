using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class NewDishFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Niacin",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "Ribofla",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "Thiamine",
                table: "dish");

            migrationBuilder.AlterColumn<double>(
                name: "Zinc",
                table: "dish",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<double>(
                name: "VitaminC",
                table: "dish",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<double>(
                name: "VitaminB6",
                table: "dish",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<double>(
                name: "VitaminB12",
                table: "dish",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<double>(
                name: "VitaminA",
                table: "dish",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<double>(
                name: "Sodium",
                table: "dish",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<double>(
                name: "Proteins",
                table: "dish",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<double>(
                name: "Potassium",
                table: "dish",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<double>(
                name: "Phosphorus",
                table: "dish",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<double>(
                name: "Magnesium",
                table: "dish",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<double>(
                name: "Iron",
                table: "dish",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<double>(
                name: "FolicAcid",
                table: "dish",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<double>(
                name: "Fiber",
                table: "dish",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<double>(
                name: "Fat",
                table: "dish",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<double>(
                name: "Cholesterol",
                table: "dish",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<double>(
                name: "Carbohydrates",
                table: "dish",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<double>(
                name: "Calories",
                table: "dish",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<double>(
                name: "Calcium",
                table: "dish",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "dish",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MonoUnsaturatedFat",
                table: "dish",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "NetWeight",
                table: "dish",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PolyUnsaturatedFat",
                table: "dish",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SaturatedFat",
                table: "dish",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "VitaminB1Thiamin",
                table: "dish",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "VitaminB2Riboflavin",
                table: "dish",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "VitaminB3Niacin",
                table: "dish",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "VitaminB9Folate",
                table: "dish",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "VitaminD",
                table: "dish",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "VitaminE",
                table: "dish",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "VitaminK",
                table: "dish",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Volume",
                table: "dish",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "MonoUnsaturatedFat",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "NetWeight",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "PolyUnsaturatedFat",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "SaturatedFat",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "VitaminB1Thiamin",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "VitaminB2Riboflavin",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "VitaminB3Niacin",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "VitaminB9Folate",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "VitaminD",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "VitaminE",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "VitaminK",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "Volume",
                table: "dish");

            migrationBuilder.AlterColumn<double>(
                name: "Zinc",
                table: "dish",
                type: "double",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "VitaminC",
                table: "dish",
                type: "double",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "VitaminB6",
                table: "dish",
                type: "double",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "VitaminB12",
                table: "dish",
                type: "double",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "VitaminA",
                table: "dish",
                type: "double",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Sodium",
                table: "dish",
                type: "double",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Proteins",
                table: "dish",
                type: "double",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Potassium",
                table: "dish",
                type: "double",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Phosphorus",
                table: "dish",
                type: "double",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Magnesium",
                table: "dish",
                type: "double",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Iron",
                table: "dish",
                type: "double",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "FolicAcid",
                table: "dish",
                type: "double",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Fiber",
                table: "dish",
                type: "double",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Fat",
                table: "dish",
                type: "double",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Cholesterol",
                table: "dish",
                type: "double",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Carbohydrates",
                table: "dish",
                type: "double",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Calories",
                table: "dish",
                type: "double",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Calcium",
                table: "dish",
                type: "double",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Niacin",
                table: "dish",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Ribofla",
                table: "dish",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Thiamine",
                table: "dish",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
