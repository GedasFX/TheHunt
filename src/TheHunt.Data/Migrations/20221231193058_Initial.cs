using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheHunt.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "competitions",
                columns: table => new
                {
                    channelid = table.Column<ulong>(name: "channel_id", type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    roleverifier = table.Column<ulong>(name: "role_verifier", type: "INTEGER", nullable: false),
                    spreadsheetid = table.Column<string>(name: "spreadsheet_id", type: "TEXT", maxLength: 44, nullable: false),
                    sheetname = table.Column<string>(name: "sheet_name", type: "TEXT", maxLength: 20, nullable: false),
                    sheetoverview = table.Column<int>(name: "sheet_overview", type: "INTEGER", nullable: false),
                    sheetconfig = table.Column<int>(name: "sheet_config", type: "INTEGER", nullable: false),
                    sheetmembers = table.Column<int>(name: "sheet_members", type: "INTEGER", nullable: false),
                    sheetitems = table.Column<int>(name: "sheet_items", type: "INTEGER", nullable: false),
                    sheetsubmissions = table.Column<int>(name: "sheet_submissions", type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_competitions", x => x.channelid);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "competitions");
        }
    }
}
