using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class EatCompoundDishRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "eatcompounddish",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompoundDishId = table.Column<int>(nullable: false),
                    EatId = table.Column<int>(nullable: false),
                    Qty = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eatcompounddish", x => x.Id);
                    table.ForeignKey(
                        name: "FK_eatcompounddish_compounddish_CompoundDishId",
                        column: x => x.CompoundDishId,
                        principalTable: "compounddish",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_eatcompounddish_eat_EatId",
                        column: x => x.EatId,
                        principalTable: "eat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_eatcompounddish_CompoundDishId",
                table: "eatcompounddish",
                column: "CompoundDishId");

            migrationBuilder.CreateIndex(
                name: "IX_eatcompounddish_EatId",
                table: "eatcompounddish",
                column: "EatId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "eatcompounddish");
        }
    }
}
