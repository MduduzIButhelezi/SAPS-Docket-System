using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;

namespace SAPS.DocketSystem.Api.Services;

public class TwilioOptions
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromPhoneNumber { get; set; } = string.Empty;
}

public record SmsResult(string Status, string? ProviderId = null);

public interface ISmsService
{
    Task<SmsResult> SendAsync(string phoneNumber, string message);
}

public class SmsService(IHttpClientFactory httpFactory, IOptions<TwilioOptions> options) : ISmsService
{
    public async Task<SmsResult> SendAsync(string phoneNumber, string message)
    {
        var cfg = options.Value;
        if (string.IsNullOrWhiteSpace(cfg.AccountSid) || string.IsNullOrWhiteSpace(cfg.AuthToken) || string.IsNullOrWhiteSpace(cfg.FromPhoneNumber))
            return new SmsResult("NotConfigured");

        var to = NormalizeSouthAfricanNumber(phoneNumber);
        if (to is null) return new SmsResult("InvalidPhoneNumber");

        try
        {
            var client = httpFactory.CreateClient();
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{cfg.AccountSid}:{cfg.AuthToken}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["To"] = to,
                ["From"] = cfg.FromPhoneNumber,
                ["Body"] = message
            });
            var response = await client.PostAsync($"https://api.twilio.com/2010-04-01/Accounts/{cfg.AccountSid}/Messages.json", content);
            var payload = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) return new SmsResult($"Failed:{(int)response.StatusCode}");

            var sid = System.Text.Json.JsonDocument.Parse(payload).RootElement.TryGetProperty("sid", out var p) ? p.GetString() : null;
            return new SmsResult("Sent", sid);
        }
        catch (Exception ex)
        {
            return new SmsResult($"Failed:{ex.GetType().Name}");
        }
    }

    private static string? NormalizeSouthAfricanNumber(string value)
    {
        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("0") && digits.Length == 10) return "+27" + digits[1..];
        if (digits.StartsWith("27") && digits.Length == 11) return "+" + digits;
        if (value.StartsWith("+") && digits.Length >= 10) return "+" + digits;
        return null;
    }
}
