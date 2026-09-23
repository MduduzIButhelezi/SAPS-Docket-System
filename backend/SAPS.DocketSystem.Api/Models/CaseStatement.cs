using System.ComponentModel.DataAnnotations;

namespace SAPS.DocketSystem.Api.Models;

public class CaseStatement
{
    public int Id { get; set; }

    public int CaseId { get; set; }

    public CaseDocket?Case { get; set; }

    [Required]
    public string OriginalText { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string SourceLanguage { get; set; } = "en";

    public string TranslatedText { get; set; } = string.Empty;

    [MaxLength(10)]
    public string TargetLanguage { get; set; } = "en";

    public string TranslationProvider { get; set; } =
        "Lelapa Vulavula";

    public bool TranslationReviewed { get; set; }

    public int? TranslationReviewedByUserId { get; set; }

    public User? TranslationReviewedByUser { get; set; }

    public DateTime CreatedAt { get; set; } =
        DateTime.UtcNow;

    public DateTime? ReviewedAt { get; set; }
}