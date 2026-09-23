using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAPS.DocketSystem.Api.Contracts;
using SAPS.DocketSystem.Api.Data;
using SAPS.DocketSystem.Api.Models;
using SAPS.DocketSystem.Api.Services;

namespace SAPS.DocketSystem.Api.Controllers;

[ApiController, Authorize, Route("api/closures")]
public class ClosureController(AppDbContext db, IAuditService audit) : ControllerBase
{
    [HttpGet("pending"), Authorize(Roles = "DetectiveCommander,StationCommander")]
    public async Task<IActionResult> Pending() => Ok(await db.CaseClosures.Include(x => x.CaseDocket).Where(x => x.Status == "PendingSupervisorApproval").ToListAsync());

    [HttpPost("case/{caseId:int}/recommend"), Authorize(Roles = "InvestigatingOfficer")]
    public async Task<IActionResult> Recommend(int caseId, RecommendClosureRequest r)
    {
        var c = await db.Cases.FindAsync(caseId); if (c is null) return NotFound();
        var closure = new CaseClosure { CaseDocketId = caseId, RecommendedByUserId = CurrentUserId(), ClosureReason = r.ClosureReason, InvestigationSummary = r.InvestigationSummary };
        db.CaseClosures.Add(closure); c.Status = CaseStatus.RecommendedForClosure; await db.SaveChangesAsync(); await audit.WriteAsync(User, "RECOMMEND_CLOSURE", "Case", caseId.ToString(), r.ClosureReason); return Ok(closure);
    }

    [HttpPost("{id:int}/review"), Authorize(Roles = "DetectiveCommander,StationCommander")]
    public async Task<IActionResult> Review(int id, ReviewClosureRequest r)
    {
        var closure = await db.CaseClosures.Include(x => x.CaseDocket).FirstOrDefaultAsync(x => x.Id == id); if (closure is null) return NotFound();
        closure.ReviewedByUserId = CurrentUserId(); closure.ReviewDate = DateTime.UtcNow; closure.ReviewComments = r.Comments; closure.Status = r.Approve ? "Approved" : "Rejected";
        closure.CaseDocket!.Status = r.Approve ? CaseStatus.Closed : CaseStatus.InvestigationInProgress; closure.CaseDocket.LastUpdated = DateTime.UtcNow; await db.SaveChangesAsync(); await audit.WriteAsync(User, r.Approve ? "APPROVE_CLOSURE" : "REJECT_CLOSURE", "Case", closure.CaseDocketId.ToString(), r.Comments); return Ok(closure);
    }

    private int CurrentUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
}
