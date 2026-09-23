using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAPS.DocketSystem.Api.Contracts;
using SAPS.DocketSystem.Api.Data;
using SAPS.DocketSystem.Api.Models;
using SAPS.DocketSystem.Api.Services;

namespace SAPS.DocketSystem.Api.Controllers;

[ApiController, Authorize(Roles = "NationalAdministrator,ProvincialAdministrator,StationAdministrator,StationCommander,DetectiveCommander,InvestigatingOfficer,ReceptionOfficer,EvidenceOfficer,ComplaintsOfficer,Auditor"), Route("api/reporting")]
public class ReportingController(AppDbContext db, IAuditService audit, ISmsService sms) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ReportingAttemptStatus? status) => Ok(await db.ReportingAttempts.Include(x => x.PublicUser).Include(x => x.PoliceStation).Where(x => !status.HasValue || x.Status == status).OrderByDescending(x => x.DateCreated).ToListAsync());

    [HttpPost("open")]
    public async Task<IActionResult> Open(OpenCaseRequest r)
    {
        var station = await db.PoliceStations.FirstOrDefaultAsync(x => x.Id == r.PoliceStationId && x.IsActive);
        if (station is null) return BadRequest(new { message = "Invalid police station." });

        var phone = new string(r.Citizen.PhoneNumber.Where(char.IsDigit).ToArray());
        var citizen = await db.PublicUsers.FirstOrDefaultAsync(x => x.PhoneNumber == phone);
        if (citizen is null)
        {
            citizen = new PublicUser { FirstName = r.Citizen.FirstName, Surname = r.Citizen.Surname, IdNumber = r.Citizen.IdNumber, PhoneNumber = phone, Email = r.Citizen.Email };
            db.PublicUsers.Add(citizen);
            await db.SaveChangesAsync();
        }

        var reportSeq = await db.ReportingAttempts.CountAsync(x => x.DateCreated.Year == DateTime.UtcNow.Year && x.PoliceStationId == station.Id) + 1;
        var reference = $"REPORT-{station.Code}-{DateTime.UtcNow:yyyy}-{reportSeq:000000}";
        var attempt = new ReportingAttempt { ReferenceNumber = reference, PublicUserId = citizen.Id, PoliceStationId = station.Id, CrimeCategory = r.CrimeCategory, IncidentSummary = r.IncidentSummary, IncidentDate = r.IncidentDate, IncidentLocation = r.IncidentLocation, Status = ReportingAttemptStatus.ConvertedToCase, ReviewedByUserId = CurrentUserId() };
        db.ReportingAttempts.Add(attempt);

        var caseSeq = await db.Cases.CountAsync(x => x.PoliceStationId == station.Id && x.DateRegistered.Year == DateTime.UtcNow.Year) + 1;
        var caseNumber = $"{station.Code}/{caseSeq:000000}/{DateTime.UtcNow:MM/yyyy}";
        var docket = new CaseDocket { CaseNumber = caseNumber, ReportingAttempt = attempt, PublicUserId = citizen.Id, PoliceStationId = station.Id, CrimeCategory = r.CrimeCategory, IncidentDescription = r.IncidentSummary, IncidentDateTime = r.IncidentDate, IncidentLocation = r.IncidentLocation, Priority = r.Priority };
        db.Cases.Add(docket);
        db.StatementTranslations.Add(new StatementTranslation
        {
            Case = docket,
            ReportingAttempt = attempt,
            OriginalText = r.IncidentSummary,
            OriginalLanguage = r.OriginalLanguage,
            TranslatedText = r.EnglishTranslation ?? string.Empty,
            TargetLanguage = "en",
            Provider = r.TranslationProvider ?? string.Empty,
            Status = r.TranslationStatus,
            TranslatedDate = r.TranslationStatus is "Completed" or "NotRequired" ? DateTime.UtcNow : null,
            CreatedByUserId = CurrentUserId()
        });
        await db.SaveChangesAsync();
        db.PublicCaseUpdates.Add(new PublicCaseUpdate { CaseDocketId = docket.Id, Title = "Case registered", Message = "Your report has been formally registered as a criminal case.", Status = docket.Status, CreatedByUserId = CurrentUserId() });
        await db.SaveChangesAsync();
        await audit.WriteAsync(User, "OPEN", "Case", docket.Id.ToString(), $"Case {caseNumber}");
        return Ok(new { docket.Id, docket.CaseNumber, Reference = reference });
    }

    [HttpPost("{id:int}/convert")]
    public async Task<IActionResult> Convert(int id, ConvertReportToCaseRequest r)
    {
        var attempt = await db.ReportingAttempts.Include(x => x.PublicUser).Include(x => x.PoliceStation).FirstOrDefaultAsync(x => x.Id == id);
        if (attempt is null) return NotFound();
        if (attempt.Status == ReportingAttemptStatus.ConvertedToCase) return BadRequest(new { message = "This report is already registered as a case." });
        var seq = await db.Cases.CountAsync(x => x.PoliceStationId == attempt.PoliceStationId && x.DateRegistered.Year == DateTime.UtcNow.Year) + 1;
        var caseNo = $"{attempt.PoliceStation!.Code}/{seq:000000}/{DateTime.UtcNow:MM/yyyy}";
        var c = new CaseDocket { CaseNumber = caseNo, ReportingAttemptId = attempt.Id, PublicUserId = attempt.PublicUserId, PoliceStationId = attempt.PoliceStationId, CrimeCategory = attempt.CrimeCategory, IncidentDescription = attempt.IncidentSummary, IncidentDateTime = attempt.IncidentDate, IncidentLocation = attempt.IncidentLocation, Priority = r.Priority };
        db.Cases.Add(c); attempt.Status = ReportingAttemptStatus.ConvertedToCase; attempt.ReviewedByUserId = CurrentUserId(); await db.SaveChangesAsync();
        db.PublicCaseUpdates.Add(new PublicCaseUpdate { CaseDocketId = c.Id, Title = "Case registered", Message = "Your report has been formally registered as a criminal case.", Status = c.Status, CreatedByUserId = CurrentUserId() });
        await db.SaveChangesAsync();
        var rs = await sms.SendAsync(attempt.PublicUser!.PhoneNumber, $"Your report {attempt.ReferenceNumber} is now registered as case {caseNo}. Keep both references safe.");
        await audit.WriteAsync(User, "CONVERT", "ReportingAttempt", attempt.Id.ToString(), $"Case {caseNo}; SMS {rs.Status}");
        return Ok(new { c.Id, c.CaseNumber, attempt.ReferenceNumber, smsStatus = rs.Status });
    }

    private int CurrentUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
}
