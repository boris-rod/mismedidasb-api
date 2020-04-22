using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class AddDishI18NColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameEN",
                table: "dish",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameIT",
                table: "dish",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameEN",
                table: "dish");

            migrationBuilder.DropColumn(
                name: "NameIT",
                table: "dish");
        }
    }
}
