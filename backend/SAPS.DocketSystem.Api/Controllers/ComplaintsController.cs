using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAPS.DocketSystem.Api.Contracts;
using SAPS.DocketSystem.Api.Data;
using SAPS.DocketSystem.Api.Models;
using SAPS.DocketSystem.Api.Services;

namespace SAPS.DocketSystem.Api.Controllers;

[ApiController, Authorize(Roles = "ComplaintsOfficer,StationCommander,ProvincialAdministrator,NationalAdministrator,Auditor"), Route("api/complaints")]
public class ComplaintsController(AppDbContext db, IAuditService audit, ISmsService sms) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await db.Complaints.Include(x => x.PublicUser).Include(x => x.PoliceStation).Include(x => x.AssignedToUser).OrderByDescending(x => x.DateSubmitted).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var c = await db.Complaints.Include(x => x.PublicUser).Include(x => x.PoliceStation).Include(x => x.AssignedToUser).FirstOrDefaultAsync(x => x.Id == id); if (c is null) return NotFound();
        var updates = await db.ComplaintUpdates.Where(x => x.ComplaintId == id).OrderByDescending(x => x.CreatedDate).ToListAsync(); return Ok(new { complaint = c, updates });
    }

    [HttpPost("{id:int}/assign")]
    public async Task<IActionResult> Assign(int id, AssignComplaintRequest r)
    {
        var c = await db.Complaints.FindAsync(id); var officer = await db.Users.FindAsync(r.ComplaintsOfficerId); if (c is null || officer is null || officer.Role != UserRole.ComplaintsOfficer) return BadRequest(new { message = "Invalid complaint or complaints officer." });
        if (c.OfficerComplainedAgainstUserId == officer.Id) return BadRequest(new { message = "A complaint cannot be assigned to the officer complained against." });
        c.AssignedToUserId = officer.Id; c.Status = ComplaintStatus.Assigned; await db.SaveChangesAsync(); await audit.WriteAsync(User, "ASSIGN", "Complaint", id.ToString(), officer.EmployeeNumber); return Ok(c);
    }

    [HttpPost("{id:int}/update")]
    public async Task<IActionResult> Update(int id, UpdateComplaintRequest r)
    {
        var c = await db.Complaints.Include(x => x.PublicUser).FirstOrDefaultAsync(x => x.Id == id); if (c is null) return NotFound(); c.Status = r.Status;
        var u = new ComplaintUpdate { ComplaintId = id, Message = r.Message, Status = r.Status, VisibleToPublic = r.VisibleToPublic, CreatedByUserId = CurrentUserId() }; db.ComplaintUpdates.Add(u); await db.SaveChangesAsync();
        if (r.SendSms && r.VisibleToPublic) await sms.SendAsync(c.PublicUser!.PhoneNumber, $"Complaint {c.ReferenceNumber}: {r.Message} Status: {r.Status}.");
        await audit.WriteAsync(User, "UPDATE", "Complaint", id.ToString(), r.Status.ToString()); return Ok(u);
    }

    [HttpPost("{id:int}/resolve")]
    public async Task<IActionResult> Resolve(int id, ResolveComplaintRequest r)
    {
        var c = await db.Complaints.Include(x => x.PublicUser).FirstOrDefaultAsync(x => x.Id == id); if (c is null) return NotFound(); c.InternalFindings = r.InternalFindings; c.PublicResolution = r.PublicResolution; c.Status = ComplaintStatus.Resolved; c.ResolutionDate = DateTime.UtcNow;
        db.ComplaintUpdates.Add(new ComplaintUpdate { ComplaintId = id, Message = r.PublicResolution, Status = ComplaintStatus.Resolved, VisibleToPublic = true, CreatedByUserId = CurrentUserId() }); await db.SaveChangesAsync();
        if (r.SendSms) await sms.SendAsync(c.PublicUser!.PhoneNumber, $"Complaint {c.ReferenceNumber} has been resolved. Track the complaint portal for the public resolution.");
        await audit.WriteAsync(User, "RESOLVE", "Complaint", id.ToString(), r.InternalFindings); return Ok(c);
    }

    private int CurrentUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
}
