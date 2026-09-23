using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAPS.DocketSystem.Api.Contracts;
using SAPS.DocketSystem.Api.Data;
using SAPS.DocketSystem.Api.Models;
using SAPS.DocketSystem.Api.Services;

namespace SAPS.DocketSystem.Api.Controllers;

[ApiController, Authorize, Route("api/dockets")]
public class DocketController(AppDbContext db, IAuditService audit) : ControllerBase
{
    [HttpGet("{caseId:int}/movements")]
    public async Task<IActionResult> Movements(int caseId) => Ok(await db.DocketMovements.Where(x => x.CaseDocketId == caseId).OrderByDescending(x => x.ReleasedDate).ToListAsync());

    [HttpPost("{caseId:int}/move"), Authorize(Roles = "ReceptionOfficer,InvestigatingOfficer,DetectiveCommander,StationCommander")]
    public async Task<IActionResult> Move(int caseId, DocketMoveRequest r)
    {
        if (!await db.Cases.AnyAsync(x => x.Id == caseId)) return NotFound();
        var m = new DocketMovement { CaseDocketId = caseId, FromLocation = r.FromLocation, ToLocation = r.ToLocation, ReleasedByUserId = CurrentUserId(), ReceivedByUserId = r.ReceivedByUserId, Reason = r.Reason };
        db.DocketMovements.Add(m); await db.SaveChangesAsync(); await audit.WriteAsync(User, "MOVE", "Docket", caseId.ToString(), $"{r.FromLocation} -> {r.ToLocation}"); return Ok(m);
    }

    [HttpPost("movements/{id:int}/acknowledge")]
    public async Task<IActionResult> Acknowledge(int id)
    {
        var m = await db.DocketMovements.FindAsync(id); if (m is null) return NotFound(); if (m.ReceivedByUserId.HasValue && m.ReceivedByUserId != CurrentUserId()) return Forbid(); m.ReceivedByUserId ??= CurrentUserId(); m.ReceivedDate = DateTime.UtcNow; m.Status = "Received"; await db.SaveChangesAsync(); await audit.WriteAsync(User, "ACKNOWLEDGE", "DocketMovement", id.ToString(), "Docket received"); return Ok(m);
    }

    private int CurrentUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
}
