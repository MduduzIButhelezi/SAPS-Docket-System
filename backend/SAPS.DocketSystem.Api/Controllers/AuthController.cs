using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAPS.DocketSystem.Api.Contracts;
using SAPS.DocketSystem.Api.Data;
using SAPS.DocketSystem.Api.Services;

namespace SAPS.DocketSystem.Api.Controllers;

[ApiController, Route("api/auth")]
public class AuthController(AppDbContext db, IJwtService jwt) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest r)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == r.Email && x.IsActive);
        if (user is null || !BCrypt.Net.BCrypt.Verify(r.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });
        return Ok(new LoginResponse(jwt.Create(user), user.Id, $"{user.FirstName} {user.Surname}", user.Role.ToString(), user.PoliceStationId));
    }
}
