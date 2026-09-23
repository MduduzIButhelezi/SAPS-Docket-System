using System.ComponentModel.DataAnnotations;

namespace SAPS.DocketSystem.Api.Models;

public class PoliceStation
{
    public int Id { get; set; }
    [MaxLength(20)] public string Code { get; set; } = string.Empty;
    [MaxLength(120)] public string Name { get; set; } = string.Empty;
    [MaxLength(80)] public string Province { get; set; } = "KwaZulu-Natal";
    [MaxLength(80)] public string District { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<PoliceUnit> Units { get; set; } = new List<PoliceUnit>();
}

public class PoliceUnit
{
    public int Id { get; set; }
    [MaxLength(100)] public string Name { get; set; } = string.Empty;
    public int PoliceStationId { get; set; }
    public PoliceStation? PoliceStation { get; set; }
    public bool IsActive { get; set; } = true;
}

public class User
{
    public int Id { get; set; }
    [MaxLength(30)] public string EmployeeNumber { get; set; } = string.Empty;
    [MaxLength(80)] public string FirstName { get; set; } = string.Empty;
    [MaxLength(80)] public string Surname { get; set; } = string.Empty;
    [MaxLength(50)] public string Rank { get; set; } = string.Empty;
    [MaxLength(160)] public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public int? PoliceStationId { get; set; }
    public PoliceStation? PoliceStation { get; set; }
    public int? PoliceUnitId { get; set; }
    public PoliceUnit? PoliceUnit { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; set; }
}

public class PublicUser
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? IdNumber { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool PhoneVerified { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

public class ReportingAttempt
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public int PublicUserId { get; set; }
    public PublicUser? PublicUser { get; set; }
    public int PoliceStationId { get; set; }
    public PoliceStation? PoliceStation { get; set; }
    public string CrimeCategory { get; set; } = string.Empty;
    public string IncidentSummary { get; set; } = string.Empty;
    public DateTime IncidentDate { get; set; }
    public string IncidentLocation { get; set; } = string.Empty;
    public ReportingAttemptStatus Status { get; set; } = ReportingAttemptStatus.Submitted;
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public int? ReviewedByUserId { get; set; }
    public string? ReviewNotes { get; set; }
}

public class CaseDocket
{
    public int Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public int ReportingAttemptId { get; set; }
    public ReportingAttempt? ReportingAttempt { get; set; }
    public int PublicUserId { get; set; }
    public PublicUser? PublicUser { get; set; }
    public int PoliceStationId { get; set; }
    public PoliceStation? PoliceStation { get; set; }
    public string CrimeCategory { get; set; } = string.Empty;
    public string IncidentDescription { get; set; } = string.Empty;
    public DateTime IncidentDateTime { get; set; }
    public string IncidentLocation { get; set; } = string.Empty;
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
    public CaseStatus Status { get; set; } = CaseStatus.AwaitingAssignment;
    public int? CurrentInvestigatingOfficerId { get; set; }
    public User? CurrentInvestigatingOfficer { get; set; }
    public DateTime DateRegistered { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public ICollection<CaseAssignment> Assignments { get; set; } = new List<CaseAssignment>();
    public ICollection<InvestigationEntry> InvestigationEntries { get; set; } = new List<InvestigationEntry>();
    public ICollection<InvestigationTask> InvestigationTasks { get; set; } = new List<InvestigationTask>();
    public ICollection<PublicCaseUpdate> PublicUpdates { get; set; } = new List<PublicCaseUpdate>();
    public ICollection<EvidenceItem> EvidenceItems { get; set; } = new List<EvidenceItem>();
    public ICollection<StatementTranslation> StatementTranslations { get; set; } = new List<StatementTranslation>();
}

public class StatementTranslation
{
    public int Id { get; set; }
    public int? CaseId { get; set; }
    public CaseDocket? Case { get; set; }
    public int? ReportingAttemptId { get; set; }
    public ReportingAttempt? ReportingAttempt { get; set; }
    public string OriginalText { get; set; } = string.Empty;
    public string OriginalLanguage { get; set; } = string.Empty;
    public string TranslatedText { get; set; } = string.Empty;
    public string TargetLanguage { get; set; } = "en";
    public string Provider { get; set; } = string.Empty;
    public string Status { get; set; } = "NotTranslated";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? TranslatedDate { get; set; }
    public int CreatedByUserId { get; set; }
}

public class CaseAssignment
{
    public int Id { get; set; }
    public int CaseDocketId { get; set; }
    public CaseDocket? CaseDocket { get; set; }
    public int AssignedOfficerId { get; set; }
    public User? AssignedOfficer { get; set; }
    public int AssignedByUserId { get; set; }
    public User? AssignedByUser { get; set; }
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public bool Accepted { get; set; }
    public DateTime? AcceptedDate { get; set; }
    public string? AssignmentReason { get; set; }
    public string Status { get; set; } = "Assigned";
}

public class InvestigationEntry
{
    public int Id { get; set; }
    public int CaseDocketId { get; set; }
    public CaseDocket? CaseDocket { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Outcome { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
}

public class InvestigationTask
{
    public int Id { get; set; }
    public int CaseDocketId { get; set; }
    public CaseDocket? CaseDocket { get; set; }
    public int AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }
    public int AssignedByUserId { get; set; }
    public User? AssignedByUser { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
    public InvestigationTaskStatus Status { get; set; } = InvestigationTaskStatus.NotStarted;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Outcome { get; set; }
}

public class PublicCaseUpdate
{
    public int Id { get; set; }
    public int CaseDocketId { get; set; }
    public CaseDocket? CaseDocket { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public CaseStatus Status { get; set; }
    public bool SmsRequested { get; set; }
    public string SmsStatus { get; set; } = "Not requested";
    public string? SmsProviderId { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }
}

public class EvidenceItem
{
    public int Id { get; set; }
    public string EvidenceNumber { get; set; } = string.Empty;
    public int CaseDocketId { get; set; }
    public CaseDocket? CaseDocket { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string StorageLocation { get; set; } = string.Empty;
    public EvidenceStatus Status { get; set; } = EvidenceStatus.Collected;
    public DateTime CollectedDate { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }
}

public class Complaint
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public int PublicUserId { get; set; }
    public PublicUser? PublicUser { get; set; }
    public int PoliceStationId { get; set; }
    public PoliceStation? PoliceStation { get; set; }
    public int? CaseDocketId { get; set; }
    public CaseDocket? CaseDocket { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime IncidentDate { get; set; }
    public string? OfficerNameProvided { get; set; }
    public string? OfficerDescription { get; set; }
    public int? OfficerComplainedAgainstUserId { get; set; }
    public User? OfficerComplainedAgainstUser { get; set; }
    public int? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }
    public ComplaintStatus Status { get; set; } = ComplaintStatus.Submitted;
    public int EscalationLevel { get; set; }
    public string? InternalFindings { get; set; }
    public string? PublicResolution { get; set; }
    public DateTime DateSubmitted { get; set; } = DateTime.UtcNow;
    public DateTime? ResolutionDate { get; set; }
}

public class ComplaintUpdate
{
    public int Id { get; set; }
    public int ComplaintId { get; set; }
    public Complaint? Complaint { get; set; }
    public string Message { get; set; } = string.Empty;
    public ComplaintStatus Status { get; set; }
    public bool VisibleToPublic { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }
}

public class AuditLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Actor { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTime DateTime { get; set; } = DateTime.UtcNow;
}


public class DocketMovement
{
    public int Id { get; set; }
    public int CaseDocketId { get; set; }
    public CaseDocket? CaseDocket { get; set; }
    public string FromLocation { get; set; } = string.Empty;
    public string ToLocation { get; set; } = string.Empty;
    public int ReleasedByUserId { get; set; }
    public int? ReceivedByUserId { get; set; }
    public DateTime ReleasedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ReceivedDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "InTransit";
}

public class EvidenceMovement
{
    public int Id { get; set; }
    public int EvidenceItemId { get; set; }
    public EvidenceItem? EvidenceItem { get; set; }
    public string FromLocation { get; set; } = string.Empty;
    public string ToLocation { get; set; } = string.Empty;
    public int ReleasedByUserId { get; set; }
    public int? ReceivedByUserId { get; set; }
    public DateTime ReleasedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ReceivedDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "InTransit";
}

public class CaseClosure
{
    public int Id { get; set; }
    public int CaseDocketId { get; set; }
    public CaseDocket? CaseDocket { get; set; }
    public int RecommendedByUserId { get; set; }
    public DateTime RecommendationDate { get; set; } = DateTime.UtcNow;
    public string ClosureReason { get; set; } = string.Empty;
    public string InvestigationSummary { get; set; } = string.Empty;
    public string Status { get; set; } = "PendingSupervisorApproval";
    public int? ReviewedByUserId { get; set; }
    public DateTime? ReviewDate { get; set; }
    public string? ReviewComments { get; set; }
}
