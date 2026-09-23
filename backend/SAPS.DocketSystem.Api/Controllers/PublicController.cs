using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAPS.DocketSystem.Api.Contracts;
using SAPS.DocketSystem.Api.Data;
using SAPS.DocketSystem.Api.Models;
using SAPS.DocketSystem.Api.Services;

namespace SAPS.DocketSystem.Api.Controllers;

[ApiController, AllowAnonymous, Route("api/public")]
public class PublicController(AppDbContext db, ISmsService sms) : ControllerBase
{
    [HttpGet("stations")]
    public async Task<IActionResult> Stations() => Ok(await db.PoliceStations.Where(x => x.IsActive).OrderBy(x => x.Name).Select(x => new { x.Id, x.Code, x.Name, x.Province, x.District }).ToListAsync());

    [HttpPost("reports")]
    public async Task<IActionResult> Report(CreatePublicReportRequest r)
    {
        if (!await db.PoliceStations.AnyAsync(x => x.Id == r.PoliceStationId && x.IsActive)) return BadRequest(new { message = "Invalid police station." });
        var citizen = await FindOrCreateCitizen(r.Citizen);
        var station = await db.PoliceStations.FindAsync(r.PoliceStationId);
        var seq = await db.ReportingAttempts.CountAsync(x => x.DateCreated.Year == DateTime.UtcNow.Year && x.PoliceStationId == r.PoliceStationId) + 1;
        var reference = $"REPORT-{station!.Code}-{DateTime.UtcNow:yyyy}-{seq:000000}";
        var attempt = new ReportingAttempt
        {
            ReferenceNumber = reference,
            PublicUserId = citizen.Id,
            PoliceStationId = r.PoliceStationId,
            CrimeCategory = r.CrimeCategory,
            IncidentSummary = r.IncidentSummary,
            IncidentDate = r.IncidentDate,
            IncidentLocation = r.IncidentLocation
        };
        db.ReportingAttempts.Add(attempt);
        await db.SaveChangesAsync();
        var result = await sms.SendAsync(citizen.PhoneNumber, $"Police report {reference} received. This is a reporting reference, not yet an official case number. Keep it safe.");
        return Ok(new { attempt.Id, reference, smsStatus = result.Status, message = "Report submitted for police review." });
    }

    [HttpPost("track-case")]
    public async Task<IActionResult> TrackCase(TrackCaseRequest r)
    {
        var phone = Normalize(r.PhoneNumber);
        var c = await db.Cases
            .Include(x => x.PublicUser).Include(x => x.PoliceStation).Include(x => x.ReportingAttempt).Include(x => x.PublicUpdates)
            .FirstOrDefaultAsync(x => (x.CaseNumber == r.Reference || x.ReportingAttempt!.ReferenceNumber == r.Reference) && x.PublicUser!.PhoneNumber == phone);
        if (c is not null)
        {
            return Ok(new
            {
                c.CaseNumber,
                ReportingReference = c.ReportingAttempt!.ReferenceNumber,
                c.CrimeCategory,
                Status = c.Status.ToString(),
                c.DateRegistered,
                c.LastUpdated,
                Station = c.PoliceStation!.Name,
                Updates = c.PublicUpdates.OrderByDescending(x => x.CreatedDate).Select(x => new { x.Title, x.Message, Status = x.Status.ToString(), x.CreatedDate })
            });
        }

        var attempt = await db.ReportingAttempts.Include(x => x.PublicUser).Include(x => x.PoliceStation)
            .FirstOrDefaultAsync(x => x.ReferenceNumber == r.Reference && x.PublicUser!.PhoneNumber == phone);
        if (attempt is null) return NotFound(new { message = "No matching report or case found for that reference and phone number." });
        return Ok(new
        {
            CaseNumber = (string?)null,
            ReportingReference = attempt.ReferenceNumber,
            attempt.CrimeCategory,
            Status = attempt.Status.ToString(),
            DateRegistered = attempt.DateCreated,
            LastUpdated = attempt.DateCreated,
            Station = attempt.PoliceStation!.Name,
            Updates = new[] { new { Title = "Report received", Message = "Your incident report is awaiting police review. This is not yet an official criminal case number.", Status = attempt.Status.ToString(), CreatedDate = attempt.DateCreated } }
        });
    }

    [HttpPost("complaints")]
    public async Task<IActionResult> LodgeComplaint(CreateComplaintRequest r)
    {
        var station = await db.PoliceStations.FindAsync(r.PoliceStationId);
        if (station is null) return BadRequest(new { message = "Invalid police station." });
        var citizen = await FindOrCreateCitizen(r.Citizen);
        var seq = await db.Complaints.CountAsync(x => x.DateSubmitted.Year == DateTime.UtcNow.Year && x.PoliceStationId == r.PoliceStationId) + 1;
        var reference = $"COMP-{station.Code}-{DateTime.UtcNow:yyyy}-{seq:000000}";
        var complaint = new Complaint
        {
            ReferenceNumber = reference,
            PublicUserId = citizen.Id,
            PoliceStationId = station.Id,
            CaseDocketId = r.CaseDocketId,
            Category = r.Category,
            Description = string.IsNullOrWhiteSpace(r.EnglishDescription) ? r.Description : r.EnglishDescription,
            IncidentDate = r.IncidentDate,
            OfficerNameProvided = r.OfficerNameProvided,
            OfficerDescription = r.OfficerDescription
        };
        db.Complaints.Add(complaint);
        await db.SaveChangesAsync();
        var result = await sms.SendAsync(citizen.PhoneNumber, $"Your police service complaint {reference} has been received. Keep this reference number to track progress.");
        return Ok(new { complaint.Id, reference, smsStatus = result.Status, message = "Complaint submitted successfully." });
    }

    [HttpPost("track-complaint")]
    public async Task<IActionResult> TrackComplaint(TrackComplaintRequest r)
    {
        var phone = Normalize(r.PhoneNumber);
        var complaint = await db.Complaints.Include(x => x.PublicUser).Include(x => x.PoliceStation).FirstOrDefaultAsync(x => x.ReferenceNumber == r.Reference && x.PublicUser!.PhoneNumber == phone);
        if (complaint is null) return NotFound(new { message = "No matching complaint found." });
        var updates = await db.ComplaintUpdates.Where(x => x.ComplaintId == complaint.Id && x.VisibleToPublic).OrderByDescending(x => x.CreatedDate).Select(x => new { x.Message, x.Status, x.CreatedDate }).ToListAsync();
        return Ok(new
        {
            complaint.ReferenceNumber,
            complaint.Category,
            complaint.Description,
            complaint.OfficerNameProvided,
            complaint.OfficerDescription,
            complaint.Status,
            complaint.DateSubmitted,
            complaint.ResolutionDate,
            complaint.PublicResolution,
            Station = complaint.PoliceStation!.Name,
            Updates = updates
        });
    }

    private async Task<PublicUser> FindOrCreateCitizen(PublicIdentityRequest r)
    {
        var phone = Normalize(r.PhoneNumber);
        var user = await db.PublicUsers.FirstOrDefaultAsync(x => x.PhoneNumber == phone);
        if (user is not null) return user;
        user = new PublicUser { FirstName = r.FirstName, Surname = r.Surname, IdNumber = r.IdNumber, PhoneNumber = phone, Email = r.Email };
        db.PublicUsers.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private static string Normalize(string value)
    {
        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("27") && digits.Length == 11) return "0" + digits[2..];
        return digits;
    }
}
