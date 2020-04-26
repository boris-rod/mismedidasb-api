using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class FixConceptInstructions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Intructions",
                table: "concept");

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "concept",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "concept");

            migrationBuilder.AddColumn<string>(
                name: "Intructions",
                table: "concept",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);
        }
    }
}
