using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class RenameTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "userpoll");

            migrationBuilder.CreateTable(
                name: "userconcept",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: false),
                    ConceptId = table.Column<int>(nullable: false),
                    Result = table.Column<string>(nullable: true),
                    CompletedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userconcept", x => x.Id);
                    table.ForeignKey(
                        name: "FK_userconcept_concept_ConceptId",
                        column: x => x.ConceptId,
                        principalTable: "concept",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_userconcept_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_userconcept_ConceptId",
                table: "userconcept",
                column: "ConceptId");

            migrationBuilder.CreateIndex(
                name: "IX_userconcept_UserId",
                table: "userconcept",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "userconcept");

            migrationBuilder.CreateTable(
                name: "userpoll",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PollId = table.Column<int>(type: "int", nullable: false),
                    Result = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userpoll", x => x.Id);
                    table.ForeignKey(
                        name: "FK_userpoll_poll_PollId",
                        column: x => x.PollId,
                        principalTable: "poll",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_userpoll_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_userpoll_PollId",
                table: "userpoll",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_userpoll_UserId",
                table: "userpoll",
                column: "UserId");
        }
    }
}
