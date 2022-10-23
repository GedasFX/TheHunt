using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheHunt.Domain.Models;

[Table("users")]
public class User
{
    [Key, Column("id")]
    public Guid Id { get; set; }

    [MaxLength(60)]
    [Column("username")]
    public string Username { get; set; } = null!;

    [MaxLength(256)]
    [Column("password_hash")]
    public byte[] PasswordHash { get; set; } = null!;
    
    [MaxLength(8)]
    [Column("password_salt")]
    public byte[] PasswordSalt { get; set; } = null!;
    
    [Column("created_at")]
    [DefaultValue("current_timestamp at time zone 'utc'")]
    public DateTime CreatedAt { get; set; }
}