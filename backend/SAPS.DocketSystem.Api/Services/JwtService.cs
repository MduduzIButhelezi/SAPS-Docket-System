using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SAPS.DocketSystem.Api.Models;

namespace SAPS.DocketSystem.Api.Services;

public interface IJwtService
{
    string Create(User user);
}

public class JwtService(IConfiguration config) : IJwtService
{
    public string Create(User user)
    {
        var key = config["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "Jwt:Key is missing from configuration.");
        }

        /*
         * HS256 requires a key of at least 256 bits.
         * UTF-8 encoding means we need at least 32 bytes.
         */
        var keyBytes = Encoding.UTF8.GetBytes(key);

        if (keyBytes.Length < 32)
        {
            throw new InvalidOperationException(
                $"Jwt:Key is too short. HS256 requires at least 32 bytes (256 bits). " +
                $"Current key is {keyBytes.Length} bytes ({keyBytes.Length * 8} bits).");
        }

        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new(
                ClaimTypes.Name,
                $"{user.FirstName} {user.Surname}"),

            new(
                ClaimTypes.Email,
                user.Email),

            new(
                ClaimTypes.Role,
                user.Role.ToString())
        };

        /*
         * Add police station ID when the user
         * is assigned to a police station.
         */
        if (user.PoliceStationId.HasValue)
        {
            claims.Add(
                new Claim(
                    "stationId",
                    user.PoliceStationId.Value.ToString()));
        }

        /*
         * Get token expiry.
         */
        var expiryHours =
            int.TryParse(
                config["Jwt:ExpiryHours"],
                out var h)
                ? h
                : 8;

        /*
         * Create JWT.
         */
        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiryHours),
            signingCredentials:
                new SigningCredentials(
                    new SymmetricSecurityKey(keyBytes),
                    SecurityAlgorithms.HmacSha256));

        /*
         * Convert JWT to string.
         */
        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}