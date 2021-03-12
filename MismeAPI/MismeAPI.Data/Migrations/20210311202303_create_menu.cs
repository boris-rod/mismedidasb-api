using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class create_menu : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "menu",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    NameEN = table.Column<string>(nullable: true),
                    NameIT = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    DescriptionEN = table.Column<string>(nullable: true),
                    DescriptionIT = table.Column<string>(nullable: true),
                    Active = table.Column<bool>(nullable: false),
                    GroupId = table.Column<int>(nullable: true),
                    CreatedById = table.Column<int>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ModifiedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menu_user_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_menu_group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "menueat",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MenuId = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ModifiedAt = table.Column<DateTime>(nullable: false),
                    EatType = table.Column<int>(nullable: false),
                    IsBalanced = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menueat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menueat_menu_MenuId",
                        column: x => x.MenuId,
                        principalTable: "menu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "menueatcompounddish",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompoundDishId = table.Column<int>(nullable: false),
                    MenuEatId = table.Column<int>(nullable: false),
                    Qty = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menueatcompounddish", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menueatcompounddish_compounddish_CompoundDishId",
                        column: x => x.CompoundDishId,
                        principalTable: "compounddish",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_menueatcompounddish_menueat_MenuEatId",
                        column: x => x.MenuEatId,
                        principalTable: "menueat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "menueatdish",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DishId = table.Column<int>(nullable: false),
                    MenuEatId = table.Column<int>(nullable: false),
                    Qty = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menueatdish", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menueatdish_dish_DishId",
                        column: x => x.DishId,
                        principalTable: "dish",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_menueatdish_menueat_MenuEatId",
                        column: x => x.MenuEatId,
                        principalTable: "menueat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_menu_CreatedById",
                table: "menu",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_menu_GroupId",
                table: "menu",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_menueat_MenuId",
                table: "menueat",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_menueatcompounddish_CompoundDishId",
                table: "menueatcompounddish",
                column: "CompoundDishId");

            migrationBuilder.CreateIndex(
                name: "IX_menueatcompounddish_MenuEatId",
                table: "menueatcompounddish",
                column: "MenuEatId");

            migrationBuilder.CreateIndex(
                name: "IX_menueatdish_DishId",
                table: "menueatdish",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_menueatdish_MenuEatId",
                table: "menueatdish",
                column: "MenuEatId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "menueatcompounddish");

            migrationBuilder.DropTable(
                name: "menueatdish");

            migrationBuilder.DropTable(
                name: "menueat");

            migrationBuilder.DropTable(
                name: "menu");
        }
    }
}
