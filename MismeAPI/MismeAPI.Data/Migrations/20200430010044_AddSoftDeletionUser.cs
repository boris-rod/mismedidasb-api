using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class AddSoftDeletionUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MarkedForDeletion",
                table: "user",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MarkedForDeletion",
                table: "user");
        }
    }
}
