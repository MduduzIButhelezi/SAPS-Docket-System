using SAPS.DocketSystem.Api.Models;

namespace SAPS.DocketSystem.Api.Contracts;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, int UserId, string FullName, string Role, int? PoliceStationId);

public record CreateStationRequest(string Code, string Name, string Province, string District, string Address, string PhoneNumber, string? Email);
public record CreateUnitRequest(int PoliceStationId, string Name);
public record CreatePoliceUserRequest(string EmployeeNumber, string FirstName, string Surname, string Rank, string Email, string PhoneNumber, string Password, UserRole Role, int? PoliceStationId, int? PoliceUnitId);

public record PublicIdentityRequest(string FirstName, string Surname, string? IdNumber, string PhoneNumber, string? Email);
public record CreatePublicReportRequest(PublicIdentityRequest Citizen, int PoliceStationId, string CrimeCategory, string IncidentSummary, DateTime IncidentDate, string IncidentLocation);
public record TrackCaseRequest(string Reference, string PhoneNumber);
public record ConvertReportToCaseRequest(PriorityLevel Priority);

public record CreateCaseRequest(int PublicUserId, int PoliceStationId, string CrimeCategory, string IncidentDescription, DateTime IncidentDateTime, string IncidentLocation, PriorityLevel Priority);
public record OpenCaseRequest(PublicIdentityRequest Citizen, int PoliceStationId, string CrimeCategory, string IncidentSummary, DateTime IncidentDate, string IncidentLocation, PriorityLevel Priority, string OriginalLanguage = "en", string? EnglishTranslation = null, string? TranslationProvider = null, string TranslationStatus = "NotTranslated");
public record AssignCaseRequest(int OfficerId, DateTime? DueDate, string? Reason);
public record AddInvestigationRequest(string ActionType, string Description, string? Outcome);
public record CreateTaskRequest(
    int CaseDocketId,
    int AssignedToUserId,
    string Title,
    string Description,
    PriorityLevel Priority,
    DateTime DueDate
);
public record CompleteTaskRequest(string Outcome);
public record PublishUpdateRequest(string Title, string Message, CaseStatus Status, bool SendSms);
public record CreateEvidenceRequest(int CaseDocketId, string Category, string Description, string StorageLocation);

public record CreateComplaintRequest(PublicIdentityRequest Citizen, int PoliceStationId, int? CaseDocketId, string Category, string Description, DateTime IncidentDate, string? OfficerNameProvided, string? OfficerDescription, string OriginalLanguage = "en", string? EnglishDescription = null, string? TranslationProvider = null, string TranslationStatus = "NotTranslated");
public record TrackComplaintRequest(string Reference, string PhoneNumber);
public record AssignComplaintRequest(int ComplaintsOfficerId);
public record UpdateComplaintRequest(ComplaintStatus Status, string Message, bool VisibleToPublic, bool SendSms);
public record ResolveComplaintRequest(string InternalFindings, string PublicResolution, bool SendSms);

public record DocketMoveRequest(string FromLocation, string ToLocation, int? ReceivedByUserId, string Reason);
public record AcknowledgeMovementRequest();
public record EvidenceMoveRequest(string FromLocation, string ToLocation, int? ReceivedByUserId, string Reason);
public record RecommendClosureRequest(string ClosureReason, string InvestigationSummary);
public record ReviewClosureRequest(bool Approve, string Comments);
