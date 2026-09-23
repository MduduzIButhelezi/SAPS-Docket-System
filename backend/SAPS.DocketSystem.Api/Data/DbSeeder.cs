using Microsoft.EntityFrameworkCore;
using SAPS.DocketSystem.Api.Models;

namespace SAPS.DocketSystem.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS StatementTranslations (
                Id INTEGER NOT NULL CONSTRAINT PK_StatementTranslations PRIMARY KEY AUTOINCREMENT,
                CaseId INTEGER NULL,
                ReportingAttemptId INTEGER NULL,
                OriginalText TEXT NOT NULL,
                OriginalLanguage TEXT NOT NULL,
                TranslatedText TEXT NOT NULL,
                TargetLanguage TEXT NOT NULL,
                Provider TEXT NOT NULL,
                Status TEXT NOT NULL,
                CreatedDate TEXT NOT NULL,
                TranslatedDate TEXT NULL,
                CreatedByUserId INTEGER NOT NULL
            );
            CREATE INDEX IF NOT EXISTS IX_StatementTranslations_CaseId ON StatementTranslations (CaseId);
            CREATE INDEX IF NOT EXISTS IX_StatementTranslations_ReportingAttemptId ON StatementTranslations (ReportingAttemptId);
            """);

        if (!await db.PoliceStations.AnyAsync())
        {
            var station = new PoliceStation
            {
                Code = "DBNC",
                Name = "Durban Central SAPS",
                Province = "KwaZulu-Natal",
                District = "eThekwini",
                Address = "Durban Central",
                PhoneNumber = "0310000000"
            };
            db.PoliceStations.Add(station);
            await db.SaveChangesAsync();

            db.PoliceUnits.AddRange(
                new PoliceUnit { Name = "Community Service Centre", PoliceStationId = station.Id },
                new PoliceUnit { Name = "Detective Unit", PoliceStationId = station.Id },
                new PoliceUnit { Name = "Evidence Management", PoliceStationId = station.Id },
                new PoliceUnit { Name = "Complaints Management", PoliceStationId = station.Id }
            );
            await db.SaveChangesAsync();
        }

        if (!await db.Users.AnyAsync())
        {
            var station = await db.PoliceStations.FirstAsync();
            var detective = await db.PoliceUnits.FirstAsync(x => x.Name == "Detective Unit");
            var csc = await db.PoliceUnits.FirstAsync(x => x.Name == "Community Service Centre");
            var complaints = await db.PoliceUnits.FirstAsync(x => x.Name == "Complaints Management");
            var evidence = await db.PoliceUnits.FirstAsync(x => x.Name == "Evidence Management");

            var password = BCrypt.Net.BCrypt.HashPassword("Admin123!");
            db.Users.AddRange(
                new User { EmployeeNumber = "ADMIN001", FirstName = "System", Surname = "Administrator", Rank = "Administrator", Email = "admin@saps.local", PhoneNumber = "0820000000", PasswordHash = password, Role = UserRole.NationalAdministrator },
                new User { EmployeeNumber = "CMD001", FirstName = "Thabo", Surname = "Dlamini", Rank = "Captain", Email = "commander@saps.local", PhoneNumber = "0820000001", PasswordHash = password, Role = UserRole.DetectiveCommander, PoliceStationId = station.Id, PoliceUnitId = detective.Id },
                new User { EmployeeNumber = "DET001", FirstName = "Sipho", Surname = "Mthembu", Rank = "Sergeant", Email = "detective@saps.local", PhoneNumber = "0820000002", PasswordHash = password, Role = UserRole.InvestigatingOfficer, PoliceStationId = station.Id, PoliceUnitId = detective.Id },
                new User { EmployeeNumber = "CSC001", FirstName = "Nandi", Surname = "Khumalo", Rank = "Constable", Email = "reception@saps.local", PhoneNumber = "0820000003", PasswordHash = password, Role = UserRole.ReceptionOfficer, PoliceStationId = station.Id, PoliceUnitId = csc.Id },
                new User { EmployeeNumber = "COMP001", FirstName = "Ayanda", Surname = "Zulu", Rank = "Sergeant", Email = "complaints@saps.local", PhoneNumber = "0820000004", PasswordHash = password, Role = UserRole.ComplaintsOfficer, PoliceStationId = station.Id, PoliceUnitId = complaints.Id },
                new User { EmployeeNumber = "EVID001", FirstName = "Lwazi", Surname = "Ngcobo", Rank = "Constable", Email = "evidence@saps.local", PhoneNumber = "0820000005", PasswordHash = password, Role = UserRole.EvidenceOfficer, PoliceStationId = station.Id, PoliceUnitId = evidence.Id }
            );
            await db.SaveChangesAsync();
        }
    }
}
