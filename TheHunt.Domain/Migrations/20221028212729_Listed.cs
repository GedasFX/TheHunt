using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheHunt.Domain.Migrations
{
    public partial class Listed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_listed",
                table: "competitions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_listed",
                table: "competitions");
        }
    }
}
