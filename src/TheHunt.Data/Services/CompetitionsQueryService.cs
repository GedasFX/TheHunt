using Microsoft.EntityFrameworkCore;
using TheHunt.Data.Models;

namespace TheHunt.Data.Services;

public class CompetitionsQueryService(AppDbContext dbContext)
{
    public async Task<Competition?> GetCompetition(ulong competitionId)
    {
        return await dbContext.Competitions.AsNoTracking()
            .Where(c => c.ChannelId == competitionId)
            .FirstOrDefaultAsync();
    }

    public async Task<SheetsRef?> GetSpreadsheetRef(ulong competitionId)
    {
        return await dbContext.Competitions.AsNoTracking()
            .Where(c => c.ChannelId == competitionId)
            .Select(c => c.Spreadsheet)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> CompetitionExists(ulong competitionId)
    {
        return await dbContext.Competitions.AsNoTracking()
            .Where(c => c.ChannelId == competitionId)
            .AnyAsync();
    }
    
    public async Task<ulong> GetVerifierRoleId(ulong competitionId)
    {
        return await dbContext.Competitions.AsNoTracking()
            .Where(c => c.ChannelId == competitionId)
            .Select(c => c.VerifierRoleId)
            .FirstOrDefaultAsync();
    }
}