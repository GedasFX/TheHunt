namespace TheHunt.Sheets.Models;

public record CompetitionUser
{
    public ulong UserId { get; init; }
    public int RowIdx { get; init; }
}