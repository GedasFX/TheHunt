namespace TheHunt.Domain.Models;

public class CompetitionUser
{
    public ulong UserId { get; set; }
    public byte Role { get; set; }
    public int RowIdx { get; set; }
}