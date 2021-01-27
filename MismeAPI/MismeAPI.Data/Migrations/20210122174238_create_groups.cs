using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class create_groups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "user",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "group",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    AdminEmail = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ModifiedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_group", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "groupinvitation",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: true),
                    UserEmail = table.Column<string>(nullable: true),
                    GroupId = table.Column<int>(nullable: true),
                    SecurityToken = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ModifiedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_groupinvitation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_groupinvitation_group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_groupinvitation_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_GroupId",
                table: "user",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_groupinvitation_GroupId",
                table: "groupinvitation",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_groupinvitation_UserId",
                table: "groupinvitation",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_user_group_GroupId",
                table: "user",
                column: "GroupId",
                principalTable: "group",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_group_GroupId",
                table: "user");

            migrationBuilder.DropTable(
                name: "groupinvitation");

            migrationBuilder.DropTable(
                name: "group");

            migrationBuilder.DropIndex(
                name: "IX_user_GroupId",
                table: "user");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "user");
        }
    }
}
