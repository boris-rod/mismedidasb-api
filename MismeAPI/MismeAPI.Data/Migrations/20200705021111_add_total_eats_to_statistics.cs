using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class add_total_eats_to_statistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalBalancedEatsPlanned",
                table: "userstatistics",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalNonBalancedEatsPlanned",
                table: "userstatistics",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalBalancedEatsPlanned",
                table: "userstatistics");

            migrationBuilder.DropColumn(
                name: "TotalNonBalancedEatsPlanned",
                table: "userstatistics");
        }
    }
}
