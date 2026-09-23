using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAPS.DocketSystem.Api.Migrations;

public partial class AddStatementTranslations : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "StatementTranslations",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                CaseId = table.Column<int>(type: "INTEGER", nullable: true),
                ReportingAttemptId = table.Column<int>(type: "INTEGER", nullable: true),
                OriginalText = table.Column<string>(type: "TEXT", nullable: false),
                OriginalLanguage = table.Column<string>(type: "TEXT", nullable: false),
                TranslatedText = table.Column<string>(type: "TEXT", nullable: false),
                TargetLanguage = table.Column<string>(type: "TEXT", nullable: false),
                Provider = table.Column<string>(type: "TEXT", nullable: false),
                Status = table.Column<string>(type: "TEXT", nullable: false),
                CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                TranslatedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StatementTranslations", x => x.Id);
            });

        migrationBuilder.CreateIndex(name: "IX_StatementTranslations_CaseId", table: "StatementTranslations", column: "CaseId");
        migrationBuilder.CreateIndex(name: "IX_StatementTranslations_ReportingAttemptId", table: "StatementTranslations", column: "ReportingAttemptId");
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "StatementTranslations");
}
