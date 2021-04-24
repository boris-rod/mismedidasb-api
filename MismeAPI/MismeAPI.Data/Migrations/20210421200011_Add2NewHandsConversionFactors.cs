using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class Add2NewHandsConversionFactors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ConversionFactor19Code",
                table: "handconversionfactor",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ConversionFactor4Code",
                table: "handconversionfactor",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConversionFactor19Code",
                table: "handconversionfactor");

            migrationBuilder.DropColumn(
                name: "ConversionFactor4Code",
                table: "handconversionfactor");
        }
    }
}
