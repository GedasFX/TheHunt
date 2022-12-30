using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheHunt.Domain.Migrations
{
    public partial class Comp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "spreadsheet_id",
                table: "competitions",
                type: "character varying(44)",
                maxLength: 44,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "sheet_name",
                table: "competitions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sheet_name",
                table: "competitions");

            migrationBuilder.AlterColumn<string>(
                name: "spreadsheet_id",
                table: "competitions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(44)",
                oldMaxLength: 44);
        }
    }
}
