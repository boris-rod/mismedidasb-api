using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class RemovePersonalDataFromConcept : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequirePersonalData",
                table: "concept");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "poll",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "poll");

            migrationBuilder.AddColumn<bool>(
                name: "RequirePersonalData",
                table: "concept",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
