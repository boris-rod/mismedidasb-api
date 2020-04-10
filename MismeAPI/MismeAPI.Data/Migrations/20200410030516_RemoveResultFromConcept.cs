using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class RemoveResultFromConcept : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Result",
                table: "userconcept");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Result",
                table: "userconcept",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);
        }
    }
}
