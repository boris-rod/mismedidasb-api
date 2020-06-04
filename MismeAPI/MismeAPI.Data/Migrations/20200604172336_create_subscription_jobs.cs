using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MismeAPI.Data.Migrations
{
    public partial class create_subscription_jobs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "usersubscriptionschedule",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ScheduleId = table.Column<int>(nullable: false),
                    UserSubscriptionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usersubscriptionschedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_usersubscriptionschedule_schedule_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "schedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usersubscriptionschedule_usersubscription_UserSubscriptionId",
                        column: x => x.UserSubscriptionId,
                        principalTable: "usersubscription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_usersubscriptionschedule_ScheduleId",
                table: "usersubscriptionschedule",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_usersubscriptionschedule_UserSubscriptionId",
                table: "usersubscriptionschedule",
                column: "UserSubscriptionId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "usersubscriptionschedule");
        }
    }
}
