using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class RemovePersonalDataInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "userpersonaldata");

            migrationBuilder.DropTable(
                name: "personaldata");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "personaldata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CodeName = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    MeasureUnit = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Name = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personaldata", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "userpersonaldata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MeasuredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PersonalDataId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userpersonaldata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_userpersonaldata_personaldata_PersonalDataId",
                        column: x => x.PersonalDataId,
                        principalTable: "personaldata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_userpersonaldata_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_userpersonaldata_PersonalDataId",
                table: "userpersonaldata",
                column: "PersonalDataId");

            migrationBuilder.CreateIndex(
                name: "IX_userpersonaldata_UserId",
                table: "userpersonaldata",
                column: "UserId");
        }
    }
}
