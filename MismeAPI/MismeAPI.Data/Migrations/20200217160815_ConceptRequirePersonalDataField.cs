using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class ConceptRequirePersonalDataField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequirePersonalData",
                table: "Concept",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "UserPoll",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: false),
                    PollId = table.Column<int>(nullable: false),
                    Result = table.Column<string>(nullable: true),
                    CompletedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPoll", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPoll_Poll_PollId",
                        column: x => x.PollId,
                        principalTable: "Poll",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPoll_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPoll_PollId",
                table: "UserPoll",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPoll_UserId",
                table: "UserPoll",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPoll");

            migrationBuilder.DropColumn(
                name: "RequirePersonalData",
                table: "Concept");
        }
    }
}
