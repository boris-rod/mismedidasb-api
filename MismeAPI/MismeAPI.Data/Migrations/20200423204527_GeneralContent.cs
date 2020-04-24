using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class GeneralContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TermsAndConditionsAccepted",
                table: "user",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "generalcontent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    ContentEN = table.Column<string>(nullable: true),
                    ContentIT = table.Column<string>(nullable: true),
                    ContentType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_generalcontent", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "generalcontent");

            migrationBuilder.DropColumn(
                name: "TermsAndConditionsAccepted",
                table: "user");
        }
    }
}
