﻿namespace TheHunt.Sheets.Models;

public record CompetitionUser
{
    public int RowIdx { get; init; }
    public ulong UserId { get; init; }
}