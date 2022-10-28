using Google.Protobuf;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheHunt.Domain;

namespace TheHunt.Api.Controllers;

[ApiController]
[Route("users")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IDataProtector _protector;

    public UserController(AppDbContext dbContext, IDataProtectionProvider dataProtectionProvider)
    {
        _dbContext = dbContext;
        _protector = dataProtectionProvider.CreateProtector("TheHunt.Api");
    }

    [HttpPost("token")]
    public async Task<IActionResult> GetToken([FromBody] UserCredentialsDto credentials)
    {
        var user = await _dbContext.Users
            .Where(u => u.Username == credentials.Username)
            .Select(u => new { u.Id, u.Password })
            .AsNoTracking().FirstOrDefaultAsync();

        if (user == null || !BCrypt.Net.BCrypt.Verify(credentials.Password, user.Password))
            return Forbid();

        _dbContext.Attach(new Domain.Models.User() { Id = user.Id, LastLogin = DateTime.UtcNow }).Property(e => e.LastLogin).IsModified = true;
        await _dbContext.SaveChangesAsync();

        return Ok(new { token = _protector.Protect(new UserData { UserId = ByteString.CopyFrom(user.Id.ToByteArray()), Username = credentials.Username }.ToByteArray()) });
    }
}