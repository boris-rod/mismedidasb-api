using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class AddHandConversionFactorEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HandConversionFactors",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Gender = table.Column<int>(nullable: false),
                    Height = table.Column<int>(nullable: false),
                    ConversionFactor = table.Column<double>(nullable: false),
                    ConversionFactor3Code = table.Column<double>(nullable: false),
                    ConversionFactor6Code = table.Column<double>(nullable: false),
                    ConversionFactor10Code = table.Column<double>(nullable: false),
                    ConversionFactor11Code = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HandConversionFactors", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HandConversionFactors");
        }
    }
}
