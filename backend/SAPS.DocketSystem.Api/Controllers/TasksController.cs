using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAPS.DocketSystem.Api.Contracts;
using SAPS.DocketSystem.Api.Data;
using SAPS.DocketSystem.Api.Models;
using SAPS.DocketSystem.Api.Services;

namespace SAPS.DocketSystem.Api.Controllers;

[ApiController, Authorize, Route("api/tasks")]
public class TasksController(AppDbContext db, IAuditService audit) : ControllerBase
{
    [HttpGet("mine")]
    public async Task<IActionResult> Mine()
    {
        var uid = CurrentUserId(); var tasks = await db.InvestigationTasks.Include(x => x.CaseDocket).Where(x => x.AssignedToUserId == uid).OrderBy(x => x.DueDate).ToListAsync();
        foreach (var t in tasks.Where(x => x.Status != InvestigationTaskStatus.Completed && x.Status != InvestigationTaskStatus.Cancelled && x.DueDate < DateTime.UtcNow)) t.Status = InvestigationTaskStatus.Overdue;
        await db.SaveChangesAsync(); return Ok(tasks);
    }

    [HttpPost, Authorize(Roles = "InvestigatingOfficer,DetectiveCommander,StationCommander")]
    public async Task<IActionResult> Create(CreateTaskRequest r)
    {
        var t = new InvestigationTask { CaseDocketId = r.CaseDocketId, AssignedToUserId = r.AssignedToUserId, AssignedByUserId = CurrentUserId(), Title = r.Title, Description = r.Description, Priority = r.Priority, DueDate = r.DueDate };
        db.InvestigationTasks.Add(t); await db.SaveChangesAsync(); await audit.WriteAsync(User, "CREATE", "InvestigationTask", t.Id.ToString(), t.Title); return Ok(t);
    }

    [HttpPost("{id:int}/start")]
    public async Task<IActionResult> Start(int id)
    {
        var t = await db.InvestigationTasks.FindAsync(id); if (t is null || t.AssignedToUserId != CurrentUserId()) return NotFound(); t.Status = InvestigationTaskStatus.InProgress; await db.SaveChangesAsync(); return Ok(t);
    }

    [HttpPost("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id, CompleteTaskRequest r)
    {
        var t = await db.InvestigationTasks.FindAsync(id); if (t is null || t.AssignedToUserId != CurrentUserId()) return NotFound(); t.Status = InvestigationTaskStatus.Completed; t.CompletedDate = DateTime.UtcNow; t.Outcome = r.Outcome; await db.SaveChangesAsync(); await audit.WriteAsync(User, "COMPLETE", "InvestigationTask", id.ToString(), r.Outcome); return Ok(t);
    }

    private int CurrentUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
}
