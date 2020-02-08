using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class MinorChangesInPersonalDataModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserPersonalData");

            migrationBuilder.AddColumn<DateTime>(
                name: "MeasuredAt",
                table: "UserPersonalData",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "UserPersonalData",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeName",
                table: "PersonalData",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "PersonalData",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "PersonalData",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MeasuredAt",
                table: "UserPersonalData");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "UserPersonalData");

            migrationBuilder.DropColumn(
                name: "CodeName",
                table: "PersonalData");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "PersonalData");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "PersonalData");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UserPersonalData",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
