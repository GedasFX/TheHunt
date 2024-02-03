using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheHunt.Data.Migrations
{
    /// <inheritdoc />
    public partial class ItemsFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "feature_items_restricted",
                table: "competitions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "feature_items_restricted",
                table: "competitions");
        }
    }
}
