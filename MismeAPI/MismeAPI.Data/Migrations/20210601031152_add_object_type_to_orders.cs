using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class add_object_type_to_orders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StripeId",
                table: "service");

            migrationBuilder.AddColumn<string>(
                name: "ObjectType",
                table: "orders",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ValidAt",
                table: "groupserviceprice",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ObjectType",
                table: "orders");

            migrationBuilder.AddColumn<string>(
                name: "StripeId",
                table: "service",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ValidAt",
                table: "groupserviceprice",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);
        }
    }
}
