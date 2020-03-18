using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class AddDeviceType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "device",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "device");
        }
    }
}
