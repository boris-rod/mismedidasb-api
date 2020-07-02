using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class AddImcKcalToEats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ImcAtThatMoment",
                table: "eat",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "KCalAtThatMoment",
                table: "eat",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImcAtThatMoment",
                table: "eat");

            migrationBuilder.DropColumn(
                name: "KCalAtThatMoment",
                table: "eat");
        }
    }
}
