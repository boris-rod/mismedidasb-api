using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class Addi18Nfields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentEN",
                table: "tip",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentIT",
                table: "tip",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleEN",
                table: "question",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleIT",
                table: "question",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEN",
                table: "poll",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionIT",
                table: "poll",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HtmlContentEN",
                table: "poll",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HtmlContentIT",
                table: "poll",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEN",
                table: "poll",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameIT",
                table: "poll",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEN",
                table: "concept",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionIT",
                table: "concept",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleEN",
                table: "concept",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleIT",
                table: "concept",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleEN",
                table: "answer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleIT",
                table: "answer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentEN",
                table: "tip");

            migrationBuilder.DropColumn(
                name: "ContentIT",
                table: "tip");

            migrationBuilder.DropColumn(
                name: "TitleEN",
                table: "question");

            migrationBuilder.DropColumn(
                name: "TitleIT",
                table: "question");

            migrationBuilder.DropColumn(
                name: "DescriptionEN",
                table: "poll");

            migrationBuilder.DropColumn(
                name: "DescriptionIT",
                table: "poll");

            migrationBuilder.DropColumn(
                name: "HtmlContentEN",
                table: "poll");

            migrationBuilder.DropColumn(
                name: "HtmlContentIT",
                table: "poll");

            migrationBuilder.DropColumn(
                name: "NameEN",
                table: "poll");

            migrationBuilder.DropColumn(
                name: "NameIT",
                table: "poll");

            migrationBuilder.DropColumn(
                name: "DescriptionEN",
                table: "concept");

            migrationBuilder.DropColumn(
                name: "DescriptionIT",
                table: "concept");

            migrationBuilder.DropColumn(
                name: "TitleEN",
                table: "concept");

            migrationBuilder.DropColumn(
                name: "TitleIT",
                table: "concept");

            migrationBuilder.DropColumn(
                name: "TitleEN",
                table: "answer");

            migrationBuilder.DropColumn(
                name: "TitleIT",
                table: "answer");
        }
    }
}
