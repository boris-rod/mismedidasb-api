using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class add_real_points_to_history : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "rewardhistory");

            migrationBuilder.AddColumn<int>(
                name: "RewardPoints",
                table: "rewardhistory",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RewardPoints",
                table: "rewardhistory");

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "rewardhistory",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
