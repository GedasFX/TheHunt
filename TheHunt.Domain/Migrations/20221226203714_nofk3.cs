using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheHunt.Domain.Migrations
{
    public partial class nofk3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_competition_members_users_user_id",
                table: "competition_members");

            migrationBuilder.DropIndex(
                name: "IX_competition_members_user_id",
                table: "competition_members");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_competition_members_user_id",
                table: "competition_members",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_competition_members_users_user_id",
                table: "competition_members",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
