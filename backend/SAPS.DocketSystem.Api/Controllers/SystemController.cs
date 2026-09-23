using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAPS.DocketSystem.Api.Data;
using SAPS.DocketSystem.Api.Models;

namespace SAPS.DocketSystem.Api.Controllers;

[ApiController, Authorize, Route("api/system")]
public class SystemController(AppDbContext db) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard() => Ok(new
    {
        totalCases = await db.Cases.CountAsync(),
        awaitingAssignment = await db.Cases.CountAsync(x => x.Status == CaseStatus.AwaitingAssignment),
        activeInvestigations = await db.Cases.CountAsync(x => x.Status == CaseStatus.InvestigationInProgress),
        openComplaints = await db.Complaints.CountAsync(x => x.Status != ComplaintStatus.Resolved && x.Status != ComplaintStatus.Closed),
        overdueTasks = await db.InvestigationTasks.CountAsync(x => x.Status != InvestigationTaskStatus.Completed && x.Status != InvestigationTaskStatus.Cancelled && x.DueDate < DateTime.UtcNow),
        stations = await db.PoliceStations.CountAsync(x => x.IsActive),
        policeUsers = await db.Users.CountAsync(x => x.IsActive)
    });

    [HttpGet("audit"), Authorize(Roles = "NationalAdministrator,ProvincialAdministrator,StationAdministrator,StationCommander,Auditor")]
    public async Task<IActionResult> Audit() => Ok(await db.AuditLogs.OrderByDescending(x => x.DateTime).Take(500).ToListAsync());

    [HttpGet("roles")]
    public IActionResult Roles() => Ok(Enum.GetNames<UserRole>());

    [HttpGet("enums")]
    public IActionResult Enums() => Ok(new
    {
        roles = Enum.GetNames<UserRole>(),
        caseStatuses = Enum.GetNames<CaseStatus>(),
        priorities = Enum.GetNames<PriorityLevel>(),
        complaintStatuses = Enum.GetNames<ComplaintStatus>(),
        taskStatuses = Enum.GetNames<InvestigationTaskStatus>()
    });
}
