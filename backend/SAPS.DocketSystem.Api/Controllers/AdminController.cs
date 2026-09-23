using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAPS.DocketSystem.Api.Contracts;
using SAPS.DocketSystem.Api.Data;
using SAPS.DocketSystem.Api.Models;
using SAPS.DocketSystem.Api.Services;

namespace SAPS.DocketSystem.Api.Controllers;

[ApiController, Authorize, Route("api/admin")]
public class AdminController(AppDbContext db, IAuditService audit) : ControllerBase
{
    [HttpGet("stations")]
    public async Task<IActionResult> Stations() => Ok(await db.PoliceStations.Include(x => x.Units).OrderBy(x => x.Name).ToListAsync());

    [HttpPost("stations"), Authorize(Roles = "NationalAdministrator,ProvincialAdministrator")]
    public async Task<IActionResult> CreateStation(CreateStationRequest r)
    {
        if (await db.PoliceStations.AnyAsync(x => x.Code == r.Code)) return BadRequest(new { message = "Station code already exists." });
        var s = new PoliceStation { Code = r.Code.ToUpper(), Name = r.Name, Province = r.Province, District = r.District, Address = r.Address, PhoneNumber = r.PhoneNumber, Email = r.Email };
        db.PoliceStations.Add(s); await db.SaveChangesAsync(); await audit.WriteAsync(User, "CREATE", "PoliceStation", s.Id.ToString(), s.Name); return Ok(s);
    }

    [HttpPost("units"), Authorize(Roles = "NationalAdministrator,ProvincialAdministrator,StationAdministrator,StationCommander")]
    public async Task<IActionResult> CreateUnit(CreateUnitRequest r)
    {
        var u = new PoliceUnit { PoliceStationId = r.PoliceStationId, Name = r.Name }; db.PoliceUnits.Add(u); await db.SaveChangesAsync(); await audit.WriteAsync(User, "CREATE", "PoliceUnit", u.Id.ToString(), u.Name); return Ok(u);
    }

    [HttpGet("users")]
    public async Task<IActionResult> Users([FromQuery] int? stationId) => Ok(await db.Users.Include(x => x.PoliceStation).Include(x => x.PoliceUnit).Where(x => !stationId.HasValue || x.PoliceStationId == stationId).OrderBy(x => x.Surname).ToListAsync());

    [HttpPost("users"), Authorize(Roles = "NationalAdministrator,ProvincialAdministrator,StationAdministrator,StationCommander")]
    public async Task<IActionResult> CreateUser(CreatePoliceUserRequest r)
    {
        if (await db.Users.AnyAsync(x => x.Email == r.Email || x.EmployeeNumber == r.EmployeeNumber)) return BadRequest(new { message = "Email or employee number already exists." });
        var callerRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if ((callerRole == nameof(UserRole.StationAdministrator) || callerRole == nameof(UserRole.StationCommander)) && (r.Role == UserRole.NationalAdministrator || r.Role == UserRole.ProvincialAdministrator))
            return Forbid();
        var u = new User { EmployeeNumber = r.EmployeeNumber, FirstName = r.FirstName, Surname = r.Surname, Rank = r.Rank, Email = r.Email, PhoneNumber = r.PhoneNumber, PasswordHash = BCrypt.Net.BCrypt.HashPassword(r.Password), Role = r.Role, PoliceStationId = r.PoliceStationId, PoliceUnitId = r.PoliceUnitId, CreatedByUserId = CurrentUserId() };
        db.Users.Add(u); await db.SaveChangesAsync(); await audit.WriteAsync(User, "CREATE", "User", u.Id.ToString(), $"{u.EmployeeNumber} {u.Role}"); return Ok(u);
    }

    [HttpPatch("users/{id:int}/active"), Authorize(Roles = "NationalAdministrator,ProvincialAdministrator,StationAdministrator,StationCommander")]
    public async Task<IActionResult> Toggle(int id, [FromQuery] bool active)
    {
        var u = await db.Users.FindAsync(id); if (u is null) return NotFound(); u.IsActive = active; await db.SaveChangesAsync(); await audit.WriteAsync(User, "UPDATE", "User", id.ToString(), $"Active={active}"); return Ok(u);
    }

    private int CurrentUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
}
