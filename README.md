# Digital Police Case & Docket Management System

School/university project prototype using **Angular 20 + ASP.NET Core 9 Web API + Entity Framework Core + SQLite**.

## What is included

### Public / citizen portal
- Report an incident online without police login.
- Automatic reporting reference generation.
- Track a reporting attempt before it becomes a formal case.
- Track a registered criminal case with public-safe updates.
- Lodge a complaint against police service without returning to the station.
- Track a complaint independently using complaint reference + phone number.
- Automatic public user record creation/reuse by phone number.
- Real SMS support through Twilio when credentials are configured.

### Police / internal portal
- JWT login and role-based access.
- National Administrator, Provincial Administrator, Station Administrator, Station Commander, Detective Commander, Investigating Officer, Reception Officer, Evidence Officer, Complaints Officer and Auditor roles.
- Register police stations and station units.
- Register police staff with employee number, rank, station, unit and role.
- Public reporting queue for Reception / CSC review.
- Convert a reporting attempt into an official case number.
- Digital case list and docket details.
- Detective assignment queue.
- System officer recommendation by same station and active workload.
- Commander makes final assignment.
- Investigating officer accepts assignment.
- Automatic default investigation tasks on acceptance.
- Investigation diary.
- Investigation task progress and overdue detection.
- Public case updates and optional SMS.
- Evidence register with evidence number and barcode.
- Evidence movement API with acknowledgement.
- Physical docket movement with receiving acknowledgement.
- Independent complaint management queue.
- Complaint assignment to Complaints Officer.
- Complaint public progress updates and SMS.
- Case closure recommendation by Investigating Officer.
- Closure approval/rejection by Detective Commander / Station Commander.
- Audit trail for major actions.
- Accountability dashboard.

## Database

SQLite is used intentionally for the school prototype. The database file is generated automatically as:

`backend/SAPS.DocketSystem.Api/saps-docket.db`

No SQL Server installation is required.

## Requirements

The project is aligned to the development environment used during setup:

- .NET SDK 9.x
- Node.js 22.x
- npm 10.x
- Angular CLI 20.0.3

## Run the backend

Open `SAPS.DocketSystem.sln` in Visual Studio and set `SAPS.DocketSystem.Api` as the startup project, or use PowerShell:

```powershell
cd backend\SAPS.DocketSystem.Api
dotnet restore
dotnet run
```

The API is fixed to:

`http://localhost:5100`

The SQLite database and demo users are created automatically on first run.

## Run Angular

Open a second PowerShell window:

```powershell
cd frontend\saps-docket-ui
npm install
npm start
```

Open:

`http://localhost:4200`

## Demo police accounts

All seeded accounts use password: `Admin123!`

| Role | Email |
|---|---|
| National Administrator | admin@saps.local |
| Detective Commander | commander@saps.local |
| Investigating Officer | detective@saps.local |
| Reception Officer | reception@saps.local |
| Complaints Officer | complaints@saps.local |
| Evidence Officer | evidence@saps.local |

The sample station is **Durban Central SAPS** with Community Service Centre, Detective Unit, Evidence Management and Complaints Management units.

## Suggested demo flow

1. Open the public portal and submit **Report an Incident**.
2. Copy the generated `REPORT-...` reference.
3. Track it immediately from **Track My Case**. It should show that police review is pending.
4. Sign in as `reception@saps.local` and open **Reporting Queue**.
5. Convert the report into an official case.
6. Sign in as `commander@saps.local` and open **Assignment Queue**.
7. Assign the case to the recommended investigating officer.
8. Sign in as `detective@saps.local` and open the case.
9. Accept assignment. Default investigation tasks are created.
10. Add investigation diary entries and publish a public update.
11. Track the case from the public portal to see the public-safe timeline.
12. Lodge a public police-service complaint.
13. Sign in as `complaints@saps.local` or a commander and manage the complaint.
14. Track the complaint from the public portal.
15. Register evidence, record docket movement, and demonstrate audit history.
16. As Investigating Officer, recommend case closure; then log in as commander to approve/reject it.

## Real SMS configuration

SMS is optional. If Twilio is not configured the system still works and records SMS status as `NotConfigured`.

For development, you can put credentials in User Secrets for the backend project:

```json
{
  "Twilio": {
    "AccountSid": "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "AuthToken": "your-token",
    "FromPhoneNumber": "+1xxxxxxxxxx"
  }
}
```

The SMS service converts normal South African mobile numbers such as `0821234567` to `+27821234567` before sending.

## Machine translation configuration

Machine translation for non-English statements uses Lelapa. The API key is intentionally not stored in `appsettings.json`. Set it as a backend user secret before starting the API:

```powershell
cd backend\SAPS.DocketSystem.Api
dotnet user-secrets set "Translation:ApiKey" "your-lelapa-api-key"
dotnet run
```

English statements are copied directly without an external API call. Without a Lelapa key, non-English translation displays a configuration error and the original statement can still be saved.

## Important when changing models

This prototype uses `EnsureCreated()` rather than migrations to keep setup simple. If you change the entity models during development, stop the API and delete:

`backend/SAPS.DocketSystem.Api/saps-docket.db`

Then run the API again to recreate the database using the latest model structure.

## Project note

This is an academic prototype, not a production SAPS deployment. Real deployment would require formal security review, legal/policy validation, production identity management, records-retention controls, infrastructure hardening, accessibility testing, approved SMS infrastructure, and enterprise database/backup architecture.
