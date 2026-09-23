using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace SAPS.DocketSystem.Api.Services;

public interface ITranslationService
{
    Task<TranslationResult> TranslateToEnglishAsync(
        string text,
        string sourceLanguage,
        CancellationToken cancellationToken = default);

    Task<TranslationResult> TranslateAsync(
        string text,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default);
}

public sealed record TranslationResult(
    string OriginalText,
    string SourceLanguage,
    string TranslatedText,
    string TargetLanguage,
    string Provider,
    bool Success,
    string? ErrorMessage = null);

public sealed class TranslationOptions
{
    public string Provider { get; set; } = "Lelapa";

    public string BaseUrl { get; set; } =
        "https://api.lelapa.ai/v1/translate/process";

    public string ApiKey { get; set; } = string.Empty;
}

public sealed class TranslationService(
    HttpClient httpClient,
    IOptions<TranslationOptions> options) : ITranslationService
{
    /*
     * Mapping used by the SAPS Docket System.
     *
     * The UI uses short language codes.
     * Lelapa uses language/model codes.
     */
    private static readonly IReadOnlyDictionary<string, string>
        SupportedLanguages =
            new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = "eng_Latn",
                ["zu"] = "zul_Latn",
                ["xh"] = "xho_Latn",
                ["af"] = "afr_Latn",
                ["st"] = "sot_Latn",
                ["tn"] = "tsn_Latn",
                ["ss"] = "ssw_Latn",
                ["ts"] = "tso_Latn",
                ["nso"] = "nso_Latn",
                ["nr"] = "nbl_Latn",
                ["ve"] = "ven_Latn"
            };


    public async Task<TranslationResult> TranslateToEnglishAsync(
        string text,
        string sourceLanguage,
        CancellationToken cancellationToken = default)
        => await TranslateAsync(text, sourceLanguage, "en", cancellationToken);

    public async Task<TranslationResult> TranslateAsync(
        string text,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default)
    {
        var original = text;

        /*
         * Validate text.
         */
        if (string.IsNullOrWhiteSpace(text))
        {
            return Failure(
                original,
                sourceLanguage,
                "Text is required.");
        }


        /*
         * Normalize language code.
         */
        var normalizedLanguage =
            sourceLanguage.Trim().ToLowerInvariant();

        var normalizedTargetLanguage =
            targetLanguage.Trim().ToLowerInvariant();


        /*
         * Check whether the language is supported.
         */
        if (!SupportedLanguages.TryGetValue(
                normalizedLanguage,
                out var sourceLanguageCode))
        {
            return Failure(
                original,
                normalizedLanguage,
                "Unsupported source language.");
        }

            if (!SupportedLanguages.TryGetValue(
                normalizedTargetLanguage,
                out var targetLanguageCode))
            {
                return Failure(
                original,
                normalizedLanguage,
                "Unsupported target language.");
            }


        /*
         * If the source is already English,
         * no API call is required.
         */
        if (normalizedLanguage == normalizedTargetLanguage)
        {
            return new TranslationResult(
                OriginalText: original,
            SourceLanguage: normalizedLanguage,
                TranslatedText: original,
            TargetLanguage: normalizedTargetLanguage,
                Provider: "Identity",
                Success: true,
                ErrorMessage: null);
        }


        /*
         * Make sure the Lelapa API key exists.
         */
        if (string.IsNullOrWhiteSpace(options.Value.ApiKey))
        {
            return Failure(
                original,
                normalizedLanguage,
                "Lelapa API key is not configured.");
        }


        /*
         * Make sure the Lelapa API URL exists.
         */
        if (string.IsNullOrWhiteSpace(options.Value.BaseUrl))
        {
            return Failure(
                original,
                normalizedLanguage,
                "Lelapa translation API URL is not configured.");
        }


        try
        {
            /*
             * Prevent the request from hanging indefinitely.
             */
            httpClient.Timeout =
                TimeSpan.FromSeconds(30);


            /*
             * Build the request body expected by Lelapa.
             */
            var request =
                new LelapaTranslationRequest
                {
                    InputText = original,
                    SourceLang = sourceLanguageCode,
                    TargetLang = targetLanguageCode
                };


            /*
             * Create HTTP POST request.
             */
            using var httpRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    options.Value.BaseUrl);


            /*
             * Lelapa authentication.
             */
            httpRequest.Headers.Add(
                "X-CLIENT-TOKEN",
                options.Value.ApiKey);


            /*
             * Add JSON request body.
             */
            httpRequest.Content =
                JsonContent.Create(request);


            /*
             * Send request to Lelapa.
             */
            using var response =
                await httpClient.SendAsync(
                    httpRequest,
                    cancellationToken);


            /*
             * Read the complete response.
             *
             * This is useful when Lelapa returns an error
             * because we can show the actual response.
             */
            var responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);


            /*
             * API KEY ERROR
             */
            if (response.StatusCode ==
                HttpStatusCode.Unauthorized)
            {
                return Failure(
                    original,
                    normalizedLanguage,
                    "Lelapa API key is invalid or expired.");
            }


            /*
             * FORBIDDEN
             */
            if (response.StatusCode ==
                HttpStatusCode.Forbidden)
            {
                return Failure(
                    original,
                    normalizedLanguage,
                    "Lelapa rejected the API request. Check your API key and account permissions.");
            }


            /*
             * ENDPOINT NOT FOUND
             */
            if (response.StatusCode ==
                HttpStatusCode.NotFound)
            {
                return Failure(
                    original,
                    normalizedLanguage,
                    $"Lelapa translation endpoint was not found. HTTP 404. Response: {responseBody}");
            }


            /*
             * RATE LIMIT
             */
            if ((int)response.StatusCode == 429)
            {
                return Failure(
                    original,
                    normalizedLanguage,
                    $"Lelapa API rate limit reached. Response: {responseBody}");
            }


            /*
             * OTHER HTTP ERRORS
             */
            if (!response.IsSuccessStatusCode)
            {
                return Failure(
                    original,
                    normalizedLanguage,
                    $"Lelapa API returned HTTP {(int)response.StatusCode}: {responseBody}");
            }


            /*
             * Deserialize Lelapa response.
             *
             * Lelapa currently returns:
             *
             * {
             *   "id": "...",
             *   "translation": [
             *     {
             *       "translated_text":
             *       "Hello, I want to report a crime."
             *     }
             *   ]
             * }
             */
            LelapaTranslationResponse? result = null;

            try
            {
                result =
                    System.Text.Json.JsonSerializer
                        .Deserialize<LelapaTranslationResponse>(
                            responseBody);
            }
            catch (Exception ex)
            {
                return Failure(
                    original,
                    normalizedLanguage,
                    $"Lelapa returned an unexpected response: {responseBody}. JSON error: {ex.Message}");
            }


            /*
             * Check for empty response.
             */
            if (result is null)
            {
                return Failure(
                    original,
                    normalizedLanguage,
                    $"Lelapa returned an empty response: {responseBody}");
            }


            /*
             * Extract translated text.
             */
            var translatedText =
                ExtractTranslation(result);


            /*
             * Lelapa responded but did not provide
             * translated text.
             */
            if (string.IsNullOrWhiteSpace(translatedText))
            {
                return Failure(
                    original,
                    normalizedLanguage,
                    $"Lelapa returned no translated text. Response: {responseBody}");
            }


            /*
             * SUCCESS
             */
            return new TranslationResult(
                OriginalText: original,
                SourceLanguage: normalizedLanguage,
                TranslatedText: translatedText,
                TargetLanguage: normalizedTargetLanguage,
                Provider: "Lelapa Vulavula",
                Success: true,
                ErrorMessage: null);
        }
        catch (HttpRequestException ex)
        {
            /*
             * Network/proxy/DNS connection error.
             */
            return Failure(
                original,
                normalizedLanguage,
                $"Lelapa connection error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            /*
             * Timeout.
             */
            return Failure(
                original,
                normalizedLanguage,
                "The Lelapa translation request timed out after 30 seconds.");
        }
        catch (Exception ex)
        {
            /*
             * Unexpected application error.
             */
            return Failure(
                original,
                normalizedLanguage,
                $"Translation failed: {ex.Message}");
        }
    }


    /*
     * Extract translation from Lelapa response.
     */
    private static string? ExtractTranslation(
        LelapaTranslationResponse response)
    {
        /*
         * Lelapa returns:
         *
         * "translation": [
         *     {
         *         "translated_text": "..."
         *     }
         * ]
         */
        if (response.Translation is null ||
            response.Translation.Count == 0)
        {
            return null;
        }


        foreach (var translation
                 in response.Translation)
        {
            if (!string.IsNullOrWhiteSpace(
                    translation.TranslatedText))
            {
                return translation.TranslatedText;
            }
        }


        return null;
    }


    /*
     * Create a standard failed translation result.
     */
    private static TranslationResult Failure(
        string text,
        string sourceLanguage,
        string message)
    {
        return new TranslationResult(
            OriginalText: text,
            SourceLanguage: sourceLanguage,
            TranslatedText: string.Empty,
            TargetLanguage: "en",
            Provider: "Lelapa Vulavula",
            Success: false,
            ErrorMessage: message);
    }


    /*
     * Lelapa request model.
     */
    private sealed class LelapaTranslationRequest
    {
        [JsonPropertyName("input_text")]
        public string InputText { get; set; } =
            string.Empty;


        [JsonPropertyName("source_lang")]
        public string SourceLang { get; set; } =
            string.Empty;


        [JsonPropertyName("target_lang")]
        public string TargetLang { get; set; } =
            string.Empty;
    }


    /*
     * Lelapa response model.
     *
     * IMPORTANT:
     * The property is "translation"
     * and NOT "translations".
     */
    private sealed class LelapaTranslationResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }


        [JsonPropertyName("translation")]
        public List<LelapaTranslationItem>? Translation { get; set; }
    }


    /*
     * Individual translation returned by Lelapa.
     */
    private sealed class LelapaTranslationItem
    {
        [JsonPropertyName("translated_text")]
        public string? TranslatedText { get; set; }
    }
}