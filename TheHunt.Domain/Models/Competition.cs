using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheHunt.Domain.Models;

[Table("competitions")]
public class Competition
{
    [Key, Column("channel_id")]
    public ulong ChannelId { get; set; }

    [Column("submission_channel_id")]
    public ulong SubmissionChannelId { get; set; }

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime? EndDate { get; set; }


    public SpreadsheetReference Spreadsheet { get; set; } = null!;
}

[Owned]
public class SpreadsheetReference
{
    [Column("spreadsheet_id")]
    public string SpreadsheetId { get; set; } = null!;

    [Column("sheet_overview")]
    public int OverviewSheetId { get; set; }

    [Column("sheet_members")]
    public int MembersSheetId { get; set; }

    [Column("sheet_items")]
    public int ItemsSheetId { get; set; }

    [Column("sheet_submissions")]
    public int SubmissionsSheetId { get; set; }
}