using Microsoft.EntityFrameworkCore;
using TheHunt.Data.Models;
using TheHunt.Domain;

namespace TheHunt.Data.Services;

public class CompetitionsQueryService
{
    private readonly AppDbContext _dbContext;

    public CompetitionsQueryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Competition?> GetCompetition(ulong competitionId)
    {
        return await _dbContext.Competitions.AsNoTracking()
            .Where(c => c.ChannelId == competitionId)
            .FirstOrDefaultAsync();
    }

    public async Task<SheetsRef?> GetSpreadsheetRef(ulong competitionId)
    {
        return await _dbContext.Competitions.AsNoTracking()
            .Where(c => c.ChannelId == competitionId)
            .Select(c => c.Spreadsheet)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> CompetitionExists(ulong competitionId)
    {
        return await _dbContext.Competitions.AsNoTracking()
            .Where(c => c.ChannelId == competitionId)
            .AnyAsync();
    }
    
    public async Task<ulong> GetVerifierRoleId(ulong competitionId)
    {
        return await _dbContext.Competitions.AsNoTracking()
            .Where(c => c.ChannelId == competitionId)
            .Select(c => c.VerifierRoleId)
            .FirstOrDefaultAsync();
    }
}