using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheHunt.Domain.Migrations
{
    public partial class Comp1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "end_date",
                table: "competitions");

            migrationBuilder.DropColumn(
                name: "start_date",
                table: "competitions");

            migrationBuilder.DropColumn(
                name: "submission_channel_id",
                table: "competitions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "end_date",
                table: "competitions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "start_date",
                table: "competitions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "submission_channel_id",
                table: "competitions",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
