﻿using Microsoft.EntityFrameworkCore;
using TheHunt.Domain;
using TheHunt.Domain.Models;

namespace TheHunt.Bot.Services;

public class CompetitionsQueryService
{
    private readonly AppDbContext _dbContext;

    public CompetitionsQueryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SpreadsheetReference?> GetSpreadsheetRef(ulong competitionId)
    {
        return await _dbContext.Competitions.AsNoTracking()
            .Where(c => c.ChannelId == competitionId)
            .Select(c => c.Spreadsheet)
            .FirstOrDefaultAsync();
    }
}