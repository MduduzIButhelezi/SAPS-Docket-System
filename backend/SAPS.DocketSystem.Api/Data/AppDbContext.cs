using Microsoft.EntityFrameworkCore;
using SAPS.DocketSystem.Api.Models;

namespace SAPS.DocketSystem.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<PublicUser> PublicUsers => Set<PublicUser>();
    public DbSet<PoliceStation> PoliceStations => Set<PoliceStation>();
    public DbSet<PoliceUnit> PoliceUnits => Set<PoliceUnit>();
    public DbSet<ReportingAttempt> ReportingAttempts => Set<ReportingAttempt>();
    public DbSet<CaseDocket> Cases => Set<CaseDocket>();
    public DbSet<CaseAssignment> CaseAssignments => Set<CaseAssignment>();
    public DbSet<InvestigationEntry> InvestigationEntries => Set<InvestigationEntry>();
    public DbSet<InvestigationTask> InvestigationTasks => Set<InvestigationTask>();
    public DbSet<PublicCaseUpdate> PublicCaseUpdates => Set<PublicCaseUpdate>();
    public DbSet<EvidenceItem> EvidenceItems => Set<EvidenceItem>();
    public DbSet<Complaint> Complaints => Set<Complaint>();
    public DbSet<ComplaintUpdate> ComplaintUpdates => Set<ComplaintUpdate>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<DocketMovement> DocketMovements => Set<DocketMovement>();
    public DbSet<EvidenceMovement> EvidenceMovements => Set<EvidenceMovement>();
    public DbSet<CaseClosure> CaseClosures => Set<CaseClosure>();
    public DbSet<StatementTranslation> StatementTranslations => Set<StatementTranslation>();
    public DbSet<CaseStatement> CaseStatements =>
    Set<CaseStatement>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(x => x.Email).IsUnique();
        b.Entity<User>().HasIndex(x => x.EmployeeNumber).IsUnique();
        b.Entity<PoliceStation>().HasIndex(x => x.Code).IsUnique();
        b.Entity<CaseDocket>().HasIndex(x => x.CaseNumber).IsUnique();
        b.Entity<ReportingAttempt>().HasIndex(x => x.ReferenceNumber).IsUnique();
        b.Entity<Complaint>().HasIndex(x => x.ReferenceNumber).IsUnique();
        b.Entity<EvidenceItem>().HasIndex(x => x.EvidenceNumber).IsUnique();
        b.Entity<StatementTranslation>().HasIndex(x => x.CaseId);
        b.Entity<StatementTranslation>().HasIndex(x => x.ReportingAttemptId);

        b.Entity<User>().HasOne(x => x.PoliceStation).WithMany().HasForeignKey(x => x.PoliceStationId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<User>().HasOne(x => x.PoliceUnit).WithMany().HasForeignKey(x => x.PoliceUnitId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<CaseDocket>().HasOne(x => x.CurrentInvestigatingOfficer).WithMany().HasForeignKey(x => x.CurrentInvestigatingOfficerId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<CaseAssignment>().HasOne(x => x.AssignedOfficer).WithMany().HasForeignKey(x => x.AssignedOfficerId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<CaseAssignment>().HasOne(x => x.AssignedByUser).WithMany().HasForeignKey(x => x.AssignedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<InvestigationTask>().HasOne(x => x.AssignedToUser).WithMany().HasForeignKey(x => x.AssignedToUserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<InvestigationTask>().HasOne(x => x.AssignedByUser).WithMany().HasForeignKey(x => x.AssignedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<InvestigationEntry>().HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Complaint>().HasOne(x => x.OfficerComplainedAgainstUser).WithMany().HasForeignKey(x => x.OfficerComplainedAgainstUserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Complaint>().HasOne(x => x.AssignedToUser).WithMany().HasForeignKey(x => x.AssignedToUserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<StatementTranslation>().HasOne(x => x.Case).WithMany(x => x.StatementTranslations).HasForeignKey(x => x.CaseId).OnDelete(DeleteBehavior.SetNull);
        b.Entity<StatementTranslation>().HasOne(x => x.ReportingAttempt).WithMany().HasForeignKey(x => x.ReportingAttemptId).OnDelete(DeleteBehavior.SetNull);
    }
}
