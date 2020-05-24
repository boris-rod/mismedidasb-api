using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class update_statistics_table_name : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userstatics_user_UserId",
                table: "userstatics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_userstatics",
                table: "userstatics");

            migrationBuilder.RenameTable(
                name: "userstatics",
                newName: "userstatistics");

            migrationBuilder.RenameIndex(
                name: "IX_userstatics_UserId",
                table: "userstatistics",
                newName: "IX_userstatistics_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_userstatistics",
                table: "userstatistics",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_userstatistics_user_UserId",
                table: "userstatistics",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userstatistics_user_UserId",
                table: "userstatistics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_userstatistics",
                table: "userstatistics");

            migrationBuilder.RenameTable(
                name: "userstatistics",
                newName: "userstatics");

            migrationBuilder.RenameIndex(
                name: "IX_userstatistics_UserId",
                table: "userstatics",
                newName: "IX_userstatics_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_userstatics",
                table: "userstatics",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_userstatics_user_UserId",
                table: "userstatics",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
