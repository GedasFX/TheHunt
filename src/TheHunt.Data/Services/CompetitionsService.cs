namespace TheHunt.Data.Services;

public class CompetitionsService
{
    private readonly AppDbContext _dbContext;

    public CompetitionsService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
}