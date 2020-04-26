using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class AddConceptInstructions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstructionsEN",
                table: "concept",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstructionsIT",
                table: "concept",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Intructions",
                table: "concept",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstructionsEN",
                table: "concept");

            migrationBuilder.DropColumn(
                name: "InstructionsIT",
                table: "concept");

            migrationBuilder.DropColumn(
                name: "Intructions",
                table: "concept");
        }
    }
}
