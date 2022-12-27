using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheHunt.Domain.Models;

[Table("competition_members")]
public class CompetitionUser
{
    [Key, Column("competition_id")]
    public ulong CompetitionId { get; set; }

    [Key, Column("user_id")]
    public ulong UserId { get; set; }

    [Column("is_admin")]
    public bool IsAdmin { get; set; }

    [Column("is_moderator")]
    public bool IsModerator { get; set; }

    [Column("registration_date")]
    public DateTime RegistrationDate { get; set; }


    [ForeignKey(nameof(CompetitionId))]
    public Competition? Competition { get; set; }

    // [ForeignKey(nameof(UserId))]
    // public User? User { get; set; }
}