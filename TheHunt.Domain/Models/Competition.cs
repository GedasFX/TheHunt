using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheHunt.Domain.Models;

[Table("competitions")]
public class Competition
{
    [Key, Column("id")]
    public Guid Id { get; set; }

    [MaxLength(200)]
    [Column("name")]
    public string Name { get; set; } = null!;

    [MaxLength(2000)]
    [Column("description")]
    public string Description { get; set; } = null!;

    [Column("admin_id")]
    public ulong AdminId { get; set; }

    [ForeignKey(nameof(AdminId))]
    public User? Admin { get; set; }

    public ICollection<User>? Verifiers { get; set; }
    public ICollection<User>? Members { get; set; }
}