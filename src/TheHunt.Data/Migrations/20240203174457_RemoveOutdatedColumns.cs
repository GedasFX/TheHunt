using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheHunt.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOutdatedColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sheet_config",
                table: "competitions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "sheet_config",
                table: "competitions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
