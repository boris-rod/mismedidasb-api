using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class AddActivatedAndDisabledFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActivatedAt",
                table: "user",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DisabledAt",
                table: "user",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivatedAt",
                table: "user");

            migrationBuilder.DropColumn(
                name: "DisabledAt",
                table: "user");
        }
    }
}
