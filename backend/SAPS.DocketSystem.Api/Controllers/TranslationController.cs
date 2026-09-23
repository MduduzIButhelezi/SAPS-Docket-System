using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPS.DocketSystem.Api.Services;

namespace SAPS.DocketSystem.Api.Controllers;

[ApiController, Authorize(Roles = "NationalAdministrator,ProvincialAdministrator,StationAdministrator,StationCommander,DetectiveCommander,InvestigatingOfficer,ReceptionOfficer,EvidenceOfficer,ComplaintsOfficer,Auditor"), Route("api/translation")]
public sealed class TranslationController(ITranslationService translationService) : ControllerBase
{
    [HttpPost("to-english")]
    [AllowAnonymous]
    public async Task<IActionResult> TranslateToEnglish(TranslationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest(new { message = "Text is required." });

        var result = await translationService.TranslateToEnglishAsync(request.Text, request.SourceLanguage, cancellationToken);
        if (!result.Success && result.ErrorMessage is "Unsupported source language." or "Unsupported target language.")
            return BadRequest(new { message = result.ErrorMessage, supportedLanguages = TranslationRequest.SupportedLanguages });

        return result.Success ? Ok(result) : StatusCode(StatusCodes.Status503ServiceUnavailable, result);
    }

    [HttpPost("translate")]
    [AllowAnonymous]
    public async Task<IActionResult> Translate(TranslationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest(new { message = "Text is required." });

        var result = await translationService.TranslateAsync(request.Text, request.SourceLanguage, request.TargetLanguage, cancellationToken);
        if (!result.Success && result.ErrorMessage is "Unsupported source language." or "Unsupported target language.")
            return BadRequest(new { message = result.ErrorMessage, supportedLanguages = TranslationRequest.SupportedLanguages });

        return result.Success ? Ok(result) : StatusCode(StatusCodes.Status503ServiceUnavailable, result);
    }
}

public sealed record TranslationRequest(string Text, string SourceLanguage, string TargetLanguage = "en")
{
    public static readonly IReadOnlyDictionary<string, string> SupportedLanguages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["en"] = "English", ["zu"] = "isiZulu", ["xh"] = "isiXhosa", ["af"] = "Afrikaans", ["st"] = "Sesotho", ["tn"] = "Setswana", ["nso"] = "Sepedi", ["ss"] = "siSwati", ["ve"] = "Tshivenda", ["ts"] = "Tsonga", ["nr"] = "isiNdebele"
    };
}
