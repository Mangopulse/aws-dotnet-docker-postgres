using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Shared.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Shared.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(bool success, string? token, string? username, DateTime? expiresAt)> AuthenticateAsync(string username, string password)
    {
        try
        {
            // Simple hardcoded authentication for demo purposes
            // In production, this should validate against a user database with hashed passwords
            var adminUsername = _configuration["Admin:Username"] ?? "admin";
            var adminPassword = _configuration["Admin:Password"] ?? "admin123";

            if (username != adminUsername || password != adminPassword)
            {
                return (false, null, null, null);
            }

            var token = GenerateJwtToken(username);
            var expiresAt = DateTime.UtcNow.AddHours(24);

            return (true, token, username, expiresAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for user {Username}", username);
            return (false, null, null, null);
        }
    }

    public async Task<(bool valid, string? username)> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtKey = _configuration["JWT:Key"] ?? "your-super-secret-jwt-key-that-should-be-at-least-256-bits-long-for-security";
            var key = Encoding.UTF8.GetBytes(jwtKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["JWT:Issuer"] ?? "AdminApi",
                ValidateAudience = true,
                ValidAudience = _configuration["JWT:Audience"] ?? "AdminApiUsers",
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var username = jwtToken.Claims.First(x => x.Type == ClaimTypes.Name).Value;

            return (true, username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return (false, null);
        }
    }

    public string GenerateJwtToken(string username)
    {
        var jwtKey = _configuration["JWT:Key"] ?? "your-super-secret-jwt-key-that-should-be-at-least-256-bits-long-for-security";
        var jwtIssuer = _configuration["JWT:Issuer"] ?? "AdminApi";
        var jwtAudience = _configuration["JWT:Audience"] ?? "AdminApiUsers";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
} 