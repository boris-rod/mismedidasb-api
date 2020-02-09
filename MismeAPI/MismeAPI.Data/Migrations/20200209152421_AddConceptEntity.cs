using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class AddConceptEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Codename",
                table: "Poll",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConceptId",
                table: "Poll",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Concept",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Codename = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ModifiedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Concept", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Poll_ConceptId",
                table: "Poll",
                column: "ConceptId");

            migrationBuilder.AddForeignKey(
                name: "FK_Poll_Concept_ConceptId",
                table: "Poll",
                column: "ConceptId",
                principalTable: "Concept",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Poll_Concept_ConceptId",
                table: "Poll");

            migrationBuilder.DropTable(
                name: "Concept");

            migrationBuilder.DropIndex(
                name: "IX_Poll_ConceptId",
                table: "Poll");

            migrationBuilder.DropColumn(
                name: "Codename",
                table: "Poll");

            migrationBuilder.DropColumn(
                name: "ConceptId",
                table: "Poll");
        }
    }
}
