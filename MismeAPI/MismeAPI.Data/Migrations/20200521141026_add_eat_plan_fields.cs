using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class add_eat_plan_fields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsValanced",
                table: "eat");

            migrationBuilder.AddColumn<bool>(
                name: "IsBalanced",
                table: "eat",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsBalancedPlan",
                table: "eat",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlanCreatedAt",
                table: "eat",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBalanced",
                table: "eat");

            migrationBuilder.DropColumn(
                name: "IsBalancedPlan",
                table: "eat");

            migrationBuilder.DropColumn(
                name: "PlanCreatedAt",
                table: "eat");

            migrationBuilder.AddColumn<bool>(
                name: "IsValanced",
                table: "eat",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
