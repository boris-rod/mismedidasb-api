using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class create_lack_self_control_dishes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "lackselfcontroldishes",
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
                    table.PrimaryKey("PK_lackselfcontroldishes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lackselfcontroldishes_dish_DishId",
                        column: x => x.DishId,
                        principalTable: "dish",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lackselfcontroldishes_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_lackselfcontroldishes_DishId",
                table: "lackselfcontroldishes",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_lackselfcontroldishes_UserId",
                table: "lackselfcontroldishes",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lackselfcontroldishes");
        }
    }
}
