using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class create_reward_system_initial_db : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rewardcategory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Category = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    MaxPointsAllowed = table.Column<int>(nullable: false),
                    PointsToIncrement = table.Column<int>(nullable: false),
                    PointsToDecrement = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rewardcategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "userstatics",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: false),
                    Points = table.Column<int>(nullable: false),
                    EatCurrentStreak = table.Column<int>(nullable: false),
                    EatMaxStreak = table.Column<int>(nullable: false),
                    BalancedEatCurrentStreak = table.Column<int>(nullable: false),
                    BalancedEatMaxStreak = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ModifiedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userstatics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_userstatics_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rewardacumulate",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: false),
                    RewardCategoryId = table.Column<int>(nullable: false),
                    Points = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ModifiedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rewardacumulate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rewardacumulate_rewardcategory_RewardCategoryId",
                        column: x => x.RewardCategoryId,
                        principalTable: "rewardcategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rewardacumulate_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rewardhistory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: false),
                    RewardCategoryId = table.Column<int>(nullable: false),
                    Points = table.Column<int>(nullable: false),
                    Data = table.Column<string>(nullable: true),
                    IsPlus = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ModifiedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rewardhistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rewardhistory_rewardcategory_RewardCategoryId",
                        column: x => x.RewardCategoryId,
                        principalTable: "rewardcategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rewardhistory_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_rewardacumulate_RewardCategoryId",
                table: "rewardacumulate",
                column: "RewardCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_rewardacumulate_UserId",
                table: "rewardacumulate",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_rewardhistory_RewardCategoryId",
                table: "rewardhistory",
                column: "RewardCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_rewardhistory_UserId",
                table: "rewardhistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_userstatics_UserId",
                table: "userstatics",
                column: "UserId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rewardacumulate");

            migrationBuilder.DropTable(
                name: "rewardhistory");

            migrationBuilder.DropTable(
                name: "userstatics");

            migrationBuilder.DropTable(
                name: "rewardcategory");
        }
    }
}
