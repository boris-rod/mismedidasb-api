using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class LowerCaseHandConversionTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_HandConversionFactors",
                table: "HandConversionFactors");

            migrationBuilder.RenameTable(
                name: "HandConversionFactors",
                newName: "handconversionfactor");

            migrationBuilder.AddPrimaryKey(
                name: "PK_handconversionfactor",
                table: "handconversionfactor",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_handconversionfactor",
                table: "handconversionfactor");

            migrationBuilder.RenameTable(
                name: "handconversionfactor",
                newName: "HandConversionFactors");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HandConversionFactors",
                table: "HandConversionFactors",
                column: "Id");
        }
    }
}
