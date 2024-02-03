namespace TheHunt.Sheets.Models;

public record CompetitionItem
{
    public int RowIdx { get; init; }
    public string Name { get; init; } = null!;
}