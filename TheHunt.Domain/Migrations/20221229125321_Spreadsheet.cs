using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheHunt.Domain.Migrations
{
    public partial class Spreadsheet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "competitions");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "competitions");

            migrationBuilder.AddColumn<int>(
                name: "sheet_items",
                table: "competitions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sheet_members",
                table: "competitions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sheet_submissions",
                table: "competitions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "spreadsheet_id",
                table: "competitions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "submission_channel_id",
                table: "competitions",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sheet_items",
                table: "competitions");

            migrationBuilder.DropColumn(
                name: "sheet_members",
                table: "competitions");

            migrationBuilder.DropColumn(
                name: "sheet_submissions",
                table: "competitions");

            migrationBuilder.DropColumn(
                name: "spreadsheet_id",
                table: "competitions");

            migrationBuilder.DropColumn(
                name: "submission_channel_id",
                table: "competitions");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "competitions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "competitions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
