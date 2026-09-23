using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAPS.DocketSystem.Api.Contracts;
using SAPS.DocketSystem.Api.Data;
using SAPS.DocketSystem.Api.Models;
using SAPS.DocketSystem.Api.Services;

namespace SAPS.DocketSystem.Api.Controllers;

[ApiController, Authorize(Roles = "EvidenceOfficer,InvestigatingOfficer,DetectiveCommander,StationCommander"), Route("api/evidence")]
public class EvidenceController(AppDbContext db, IAuditService audit) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? caseId) => Ok(await db.EvidenceItems.Include(x => x.CaseDocket).Where(x => !caseId.HasValue || x.CaseDocketId == caseId).OrderByDescending(x => x.CollectedDate).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(CreateEvidenceRequest r)
    {
        if (!await db.Cases.AnyAsync(x => x.Id == r.CaseDocketId)) return BadRequest(new { message = "Invalid case." });
        var seq = await db.EvidenceItems.CountAsync(x => x.CaseDocketId == r.CaseDocketId) + 1;
        var e = new EvidenceItem { CaseDocketId = r.CaseDocketId, Category = r.Category, Description = r.Description, StorageLocation = r.StorageLocation, EvidenceNumber = $"EVID-{r.CaseDocketId:000000}-{seq:000}", Barcode = $"BC{r.CaseDocketId:000000}{seq:000}", CreatedByUserId = CurrentUserId() };
        db.EvidenceItems.Add(e); await db.SaveChangesAsync(); await audit.WriteAsync(User, "CREATE", "Evidence", e.Id.ToString(), e.EvidenceNumber); return Ok(e);
    }


    [HttpGet("{evidenceId:int}/movements")]
    public async Task<IActionResult> Movements(int evidenceId) => Ok(await db.EvidenceMovements.Where(x => x.EvidenceItemId == evidenceId).OrderByDescending(x => x.ReleasedDate).ToListAsync());

    [HttpPost("{evidenceId:int}/move")]
    public async Task<IActionResult> Move(int evidenceId, EvidenceMoveRequest r)
    {
        var e = await db.EvidenceItems.FindAsync(evidenceId); if (e is null) return NotFound();
        var m = new EvidenceMovement { EvidenceItemId = evidenceId, FromLocation = r.FromLocation, ToLocation = r.ToLocation, ReleasedByUserId = CurrentUserId(), ReceivedByUserId = r.ReceivedByUserId, Reason = r.Reason };
        db.EvidenceMovements.Add(m); e.Status = EvidenceStatus.InTransit; e.StorageLocation = r.ToLocation; await db.SaveChangesAsync(); await audit.WriteAsync(User, "MOVE", "Evidence", evidenceId.ToString(), $"{r.FromLocation} -> {r.ToLocation}"); return Ok(m);
    }

    [HttpPost("movements/{id:int}/acknowledge")]
    public async Task<IActionResult> Acknowledge(int id)
    {
        var m = await db.EvidenceMovements.Include(x => x.EvidenceItem).FirstOrDefaultAsync(x => x.Id == id); if (m is null) return NotFound();
        if (m.ReceivedByUserId.HasValue && m.ReceivedByUserId != CurrentUserId()) return Forbid();
        m.ReceivedByUserId ??= CurrentUserId(); m.ReceivedDate = DateTime.UtcNow; m.Status = "Received"; if (m.EvidenceItem is not null) m.EvidenceItem.Status = EvidenceStatus.InStorage; await db.SaveChangesAsync(); await audit.WriteAsync(User, "ACKNOWLEDGE", "EvidenceMovement", id.ToString(), "Evidence received"); return Ok(m);
    }

    private int CurrentUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
}
