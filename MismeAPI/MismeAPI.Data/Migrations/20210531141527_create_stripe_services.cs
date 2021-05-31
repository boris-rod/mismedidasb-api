using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class create_stripe_services : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "service",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    StripeId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "serviceprice",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ServiceId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    PriceId = table.Column<string>(nullable: true),
                    Price = table.Column<int>(nullable: false),
                    Currency = table.Column<string>(nullable: true),
                    Interval = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_serviceprice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_serviceprice_service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "groupserviceprice",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ServicePriceId = table.Column<int>(nullable: false),
                    GroupId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: true),
                    IsValid = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    ValidAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_groupserviceprice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_groupserviceprice_group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_groupserviceprice_serviceprice_ServicePriceId",
                        column: x => x.ServicePriceId,
                        principalTable: "serviceprice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_groupserviceprice_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_groupserviceprice_GroupId",
                table: "groupserviceprice",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_groupserviceprice_ServicePriceId",
                table: "groupserviceprice",
                column: "ServicePriceId");

            migrationBuilder.CreateIndex(
                name: "IX_groupserviceprice_UserId",
                table: "groupserviceprice",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_serviceprice_ServiceId",
                table: "serviceprice",
                column: "ServiceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "groupserviceprice");

            migrationBuilder.DropTable(
                name: "serviceprice");

            migrationBuilder.DropTable(
                name: "service");
        }
    }
}
