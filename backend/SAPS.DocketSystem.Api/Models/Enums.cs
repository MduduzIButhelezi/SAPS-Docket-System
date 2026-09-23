namespace SAPS.DocketSystem.Api.Models;

public enum UserRole
{
    NationalAdministrator,
    ProvincialAdministrator,
    StationAdministrator,
    StationCommander,
    DetectiveCommander,
    InvestigatingOfficer,
    ReceptionOfficer,
    EvidenceOfficer,
    ComplaintsOfficer,
    Auditor
}

public enum CaseStatus
{
    AwaitingAssignment,
    Assigned,
    InvestigationInProgress,
    AwaitingInformation,
    AwaitingForensics,
    AwaitingProsecutor,
    CourtProcess,
    Transferred,
    RecommendedForClosure,
    Closed,
    Archived
}

public enum PriorityLevel { Low, Medium, High, Critical }
public enum ReportingAttemptStatus { Submitted, UnderReview, ConvertedToCase, Referred, Closed }
public enum ComplaintStatus { Submitted, Acknowledged, Assigned, UnderInvestigation, AwaitingOfficerResponse, Escalated, Resolved, Closed, Reopened }
public enum InvestigationTaskStatus { NotStarted, InProgress, AwaitingInformation, Completed, Overdue, Cancelled }
public enum EvidenceStatus { Collected, Sealed, InTransit, InStorage, SubmittedForForensics, ReturnedFromForensics, PresentedInCourt, Released, Disposed }
