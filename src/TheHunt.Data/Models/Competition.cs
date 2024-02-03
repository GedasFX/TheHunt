using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheHunt.Data.Models;

[Table("competitions")]
public class Competition
{
    [Key, Column("channel_id")]
    public ulong ChannelId { get; set; }

    [Column("role_verifier")]
    public ulong VerifierRoleId { get; set; }

    public SheetsRef Spreadsheet { get; set; } = null!;

    public Features Features { get; set; } = null!;
}

[Owned]
public class SheetsRef
{
    [StringLength(44)]
    [Column("spreadsheet_id")]
    public string SpreadsheetId { get; set; } = null!;

    [StringLength(20)]
    [Column("sheet_name")]
    public string SheetName { get; set; } = null!;


    public Sheet Sheets { get; set; } = null!;

    [Owned]
    public class Sheet
    {
        [Column("sheet_overview")]
        public int Overview { get; set; }

        [Column("sheet_members")]
        public int Members { get; set; }

        [Column("sheet_items")]
        public int Items { get; set; }

        [Column("sheet_submissions")]
        public int Submissions { get; set; }
    }
}

[Owned]
public class Features
{
    [Column("feature_items_restricted")]
    public bool ItemsRestricted { get; set; }
}