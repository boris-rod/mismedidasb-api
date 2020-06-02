using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class create_subscription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "subscription",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Product = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ValueCoins = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ModifiedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usersubscription",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: false),
                    SubscriptionId = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    ValidDays = table.Column<TimeSpan>(nullable: false),
                    ValidAt = table.Column<DateTime>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ModifiedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usersubscription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_usersubscription_subscription_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "subscription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usersubscription_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_usersubscription_SubscriptionId",
                table: "usersubscription",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_usersubscription_UserId",
                table: "usersubscription",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "usersubscription");

            migrationBuilder.DropTable(
                name: "subscription");
        }
    }
}
