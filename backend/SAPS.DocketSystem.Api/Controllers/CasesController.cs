using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAPS.DocketSystem.Api.Contracts;
using SAPS.DocketSystem.Api.Data;
using SAPS.DocketSystem.Api.Models;
using SAPS.DocketSystem.Api.Services;

namespace SAPS.DocketSystem.Api.Controllers;

[ApiController, Authorize, Route("api/cases")]
public class CasesController(AppDbContext db, IAuditService audit, ISmsService sms) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] CaseStatus? status)
    {
        var q = db.Cases.Include(x => x.PublicUser).Include(x => x.PoliceStation).Include(x => x.CurrentInvestigatingOfficer).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) q = q.Where(x => x.CaseNumber.Contains(search) || x.CrimeCategory.Contains(search) || x.PublicUser!.FirstName.Contains(search) || x.PublicUser.Surname.Contains(search));
        if (status.HasValue) q = q.Where(x => x.Status == status);
        return Ok(await q.OrderByDescending(x => x.DateRegistered).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var c = await db.Cases.Include(x => x.PublicUser).Include(x => x.PoliceStation).Include(x => x.ReportingAttempt).Include(x => x.CurrentInvestigatingOfficer)
            .Include(x => x.Assignments).ThenInclude(x => x.AssignedOfficer)
            .Include(x => x.InvestigationEntries).ThenInclude(x => x.CreatedByUser)
            .Include(x => x.InvestigationTasks).ThenInclude(x => x.AssignedToUser)
            .Include(x => x.PublicUpdates).Include(x => x.EvidenceItems).FirstOrDefaultAsync(x => x.Id == id);
        return c is null ? NotFound() : Ok(c);
    }

    [HttpGet("assignment-queue"), Authorize(Roles = "DetectiveCommander,StationCommander,NationalAdministrator,ProvincialAdministrator")]
    public async Task<IActionResult> AssignmentQueue() => Ok(await db.Cases.Include(x => x.PublicUser).Include(x => x.PoliceStation).Where(x => x.Status == CaseStatus.AwaitingAssignment).OrderByDescending(x => x.Priority).ThenBy(x => x.DateRegistered).ToListAsync());

    [HttpGet("{id:int}/recommended-officers"), Authorize(Roles = "DetectiveCommander,StationCommander,NationalAdministrator,ProvincialAdministrator")]
    public async Task<IActionResult> RecommendedOfficers(int id)
    {
        var c = await db.Cases.FindAsync(id); if (c is null) return NotFound();
        var result = await db.Users.Where(x => x.IsActive && x.Role == UserRole.InvestigatingOfficer && x.PoliceStationId == c.PoliceStationId)
            .Select(x => new { x.Id, FullName = x.FirstName + " " + x.Surname, x.Rank, ActiveCases = db.CaseAssignments.Count(a => a.AssignedOfficerId == x.Id && a.Status != "Completed") })
            .OrderBy(x => x.ActiveCases).ToListAsync();
        return Ok(result);
    }

    [HttpPost("{id:int}/assign"), Authorize(Roles = "DetectiveCommander,StationCommander,NationalAdministrator,ProvincialAdministrator")]
    public async Task<IActionResult> Assign(int id, AssignCaseRequest r)
    {
        var c = await db.Cases.FindAsync(id); if (c is null) return NotFound();
        var officer = await db.Users.FirstOrDefaultAsync(x => x.Id == r.OfficerId && x.IsActive && x.Role == UserRole.InvestigatingOfficer && x.PoliceStationId == c.PoliceStationId);
        if (officer is null) return BadRequest(new { message = "Officer is not an active investigating officer at this station." });
        foreach (var old in await db.CaseAssignments.Where(x => x.CaseDocketId == id && x.Status != "Completed").ToListAsync()) old.Status = "Reassigned";
        var a = new CaseAssignment { CaseDocketId = id, AssignedOfficerId = officer.Id, AssignedByUserId = CurrentUserId(), DueDate = r.DueDate, AssignmentReason = r.Reason };
        db.CaseAssignments.Add(a); c.CurrentInvestigatingOfficerId = officer.Id; c.Status = CaseStatus.Assigned; c.LastUpdated = DateTime.UtcNow; await db.SaveChangesAsync();
        await audit.WriteAsync(User, "ASSIGN", "Case", id.ToString(), $"Assigned to {officer.EmployeeNumber}"); return Ok(a);
    }

    [HttpPost("{id:int}/accept-assignment"), Authorize(Roles = "InvestigatingOfficer")]
    public async Task<IActionResult> Accept(int id)
    {
        var uid = CurrentUserId(); var a = await db.CaseAssignments.OrderByDescending(x => x.AssignedDate).FirstOrDefaultAsync(x => x.CaseDocketId == id && x.AssignedOfficerId == uid && x.Status == "Assigned");
        if (a is null) return NotFound(new { message = "No pending assignment found." });
        a.Accepted = true; a.AcceptedDate = DateTime.UtcNow; a.Status = "Accepted";
        var c = await db.Cases.FindAsync(id); c!.Status = CaseStatus.InvestigationInProgress; c.LastUpdated = DateTime.UtcNow;
        var defaults = new[] { "Review digital docket", "Contact complainant", "Identify witnesses", "Record initial investigation update" };
        foreach (var title in defaults) db.InvestigationTasks.Add(new InvestigationTask { CaseDocketId = id, AssignedToUserId = uid, AssignedByUserId = uid, Title = title, Description = title, DueDate = DateTime.UtcNow.AddDays(3) });
        await db.SaveChangesAsync(); await audit.WriteAsync(User, "ACCEPT", "CaseAssignment", a.Id.ToString(), $"Case {id}"); return Ok(a);
    }

    [HttpPost("{id:int}/investigation"), Authorize(Roles = "InvestigatingOfficer,DetectiveCommander,StationCommander")]
    public async Task<IActionResult> AddInvestigation(int id, AddInvestigationRequest r)
    {
        var c = await db.Cases.FindAsync(id); if (c is null) return NotFound();
        var entry = new InvestigationEntry { CaseDocketId = id, ActionType = r.ActionType, Description = r.Description, Outcome = r.Outcome, CreatedByUserId = CurrentUserId() };
        db.InvestigationEntries.Add(entry); c.Status = CaseStatus.InvestigationInProgress; c.LastUpdated = DateTime.UtcNow; await db.SaveChangesAsync(); await audit.WriteAsync(User, "UPDATE", "Case", id.ToString(), "Investigation entry added"); return Ok(entry);
    }

    [HttpPost("{id:int}/public-updates"), Authorize(Roles = "InvestigatingOfficer,DetectiveCommander,StationCommander")]
    public async Task<IActionResult> Publish(int id, PublishUpdateRequest r)
    {
        var c = await db.Cases.Include(x => x.PublicUser).FirstOrDefaultAsync(x => x.Id == id); if (c is null) return NotFound();
        var smsStatus = "Not requested"; string? provider = null;
        if (r.SendSms) { var rs = await sms.SendAsync(c.PublicUser!.PhoneNumber, $"Case {c.CaseNumber}: {r.Message} Status: {r.Status}."); smsStatus = rs.Status; provider = rs.ProviderId; }
        var u = new PublicCaseUpdate { CaseDocketId = id, Title = r.Title, Message = r.Message, Status = r.Status, SmsRequested = r.SendSms, SmsStatus = smsStatus, SmsProviderId = provider, CreatedByUserId = CurrentUserId() };
        db.PublicCaseUpdates.Add(u); c.Status = r.Status; c.LastUpdated = DateTime.UtcNow; await db.SaveChangesAsync(); await audit.WriteAsync(User, "PUBLISH", "PublicCaseUpdate", u.Id.ToString(), smsStatus); return Ok(u);
    }

    private int CurrentUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
}
