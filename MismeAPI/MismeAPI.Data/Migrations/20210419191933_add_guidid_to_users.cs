using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class add_guidid_to_users : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GuidId",
                table: "user",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_GuidId",
                table: "user",
                column: "GuidId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_GuidId",
                table: "user");

            migrationBuilder.DropColumn(
                name: "GuidId",
                table: "user");
        }
    }
}
