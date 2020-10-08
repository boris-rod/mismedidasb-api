using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class create_favorite_lack_control_compound_dish : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "favoritecompounddish",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DishId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_favoritecompounddish", x => x.Id);
                    table.ForeignKey(
                        name: "FK_favoritecompounddish_compounddish_DishId",
                        column: x => x.DishId,
                        principalTable: "compounddish",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_favoritecompounddish_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lackselfcontrolcompounddishes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DishId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    Intensity = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lackselfcontrolcompounddishes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lackselfcontrolcompounddishes_compounddish_DishId",
                        column: x => x.DishId,
                        principalTable: "compounddish",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lackselfcontrolcompounddishes_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_favoritecompounddish_DishId",
                table: "favoritecompounddish",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_favoritecompounddish_UserId",
                table: "favoritecompounddish",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_lackselfcontrolcompounddishes_DishId",
                table: "lackselfcontrolcompounddishes",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_lackselfcontrolcompounddishes_UserId",
                table: "lackselfcontrolcompounddishes",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "favoritecompounddish");

            migrationBuilder.DropTable(
                name: "lackselfcontrolcompounddishes");
        }
    }
}
