using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheHunt.Domain.Models;

[Table("users")]
public class User
{
    [Key, Column("id")]
    public long Id { get; set; }

    [MaxLength(60)]
    [Column("username")]
    public string Username { get; set; } = null!;

    [MaxLength(60)]
    [Column("password")]
    public string Password { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("last_login")]
    public DateTime LastLogin { get; set; }


    public ICollection<CompetitionUser>? Competitions { get; set; }
}