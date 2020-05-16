using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class DishAndCompoundDishRelationships : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compounddish",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Image = table.Column<string>(nullable: true),
                    ImageMimeType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compounddish", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compounddish_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dishcompounddish",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DishId = table.Column<int>(nullable: false),
                    CompoundDishId = table.Column<int>(nullable: false),
                    DishQty = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dishcompounddish", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dishcompounddish_compounddish_CompoundDishId",
                        column: x => x.CompoundDishId,
                        principalTable: "compounddish",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dishcompounddish_dish_DishId",
                        column: x => x.DishId,
                        principalTable: "dish",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compounddish_UserId",
                table: "compounddish",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_dishcompounddish_CompoundDishId",
                table: "dishcompounddish",
                column: "CompoundDishId");

            migrationBuilder.CreateIndex(
                name: "IX_dishcompounddish_DishId",
                table: "dishcompounddish",
                column: "DishId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dishcompounddish");

            migrationBuilder.DropTable(
                name: "compounddish");
        }
    }
}
