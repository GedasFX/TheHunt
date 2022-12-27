using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheHunt.Domain.Models;

[Table("competitions")]
public class Competition
{
    [Key, Column("channel_id")]
    public ulong ChannelId { get; set; }

    [MaxLength(200)]
    [Column("name")]
    public string Name { get; set; } = null!;

    [MaxLength(2000)]
    [Column("description")]
    public string? Description { get; set; }

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime? EndDate { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }


    public ICollection<CompetitionUser>? Members { get; set; }
}