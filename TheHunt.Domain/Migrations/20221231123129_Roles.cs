using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheHunt.Domain.Migrations
{
    public partial class Roles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "role_participant",
                table: "competitions",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "role_verifier",
                table: "competitions",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "role_participant",
                table: "competitions");

            migrationBuilder.DropColumn(
                name: "role_verifier",
                table: "competitions");
        }
    }
}
