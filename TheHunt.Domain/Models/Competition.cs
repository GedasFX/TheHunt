using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TheHunt.Domain.Models;

[Owned]
public class SpreadsheetReference
{
    [Column("spreadsheet_id")]
    public string SpreadsheetId { get; set; } = null!;

    [Column("sheet_members")]
    public int MembersSheet { get; set; }

    [Column("sheet_items")]
    public int ItemsSheet { get; set; }

    [Column("sheet_submissions")]
    public int SubmissionsSheet { get; set; }
}

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

    // public ICollection<CompetitionUser>? Members { get; set; }
}