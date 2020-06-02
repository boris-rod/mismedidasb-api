using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class solo_question_answer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "soloquestion",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    TitleEN = table.Column<string>(nullable: true),
                    TitleIT = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ModifiedAt = table.Column<DateTime>(nullable: false),
                    AllowCustomAnswer = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_soloquestion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "soloanswer",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SoloQuestionId = table.Column<int>(nullable: false),
                    Code = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ModifiedAt = table.Column<DateTime>(nullable: false),
                    TitleEN = table.Column<string>(nullable: true),
                    TitleIT = table.Column<string>(nullable: true),
                    Points = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_soloanswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_soloanswer_soloquestion_SoloQuestionId",
                        column: x => x.SoloQuestionId,
                        principalTable: "soloquestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usersoloanswer",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: false),
                    SoloAnswerId = table.Column<int>(nullable: true),
                    QuestionCode = table.Column<string>(nullable: true),
                    AnswerCode = table.Column<string>(nullable: true),
                    AnswerValue = table.Column<string>(nullable: true),
                    Points = table.Column<int>(nullable: false),
                    Coins = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usersoloanswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_usersoloanswer_soloanswer_SoloAnswerId",
                        column: x => x.SoloAnswerId,
                        principalTable: "soloanswer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_usersoloanswer_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_soloanswer_SoloQuestionId",
                table: "soloanswer",
                column: "SoloQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_usersoloanswer_SoloAnswerId",
                table: "usersoloanswer",
                column: "SoloAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_usersoloanswer_UserId",
                table: "usersoloanswer",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "usersoloanswer");

            migrationBuilder.DropTable(
                name: "soloanswer");

            migrationBuilder.DropTable(
                name: "soloquestion");
        }
    }
}
