using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Shared.Interfaces;
using Shared.Models;

namespace Shared.Services
{
    public class AuthService : IAuthService
    {
        private readonly string _jwtKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;

        public AuthService(IConfiguration configuration)
        {
            _jwtKey = configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key is missing");
            _jwtIssuer = configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer is missing");
            _jwtAudience = configuration["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience is missing");
        }

        public async Task<(bool success, string? token, string? username, DateTime? expiresAt)> AuthenticateAsync(string username, string password)
        {
            if (username == "admin" && password == "admin123")
            {
                var token = GenerateJwtToken(username);
                var expiresAt = DateTime.UtcNow.AddHours(1);
                return (true, token, username, expiresAt);
            }

            return (false, null, null, null);
        }

        public async Task<(bool valid, string? username)> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return (false, null);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtKey);

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                var username = principal.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
                return (true, username);
            }
            catch (SecurityTokenExpiredException)
            {
                return (false, null);
            }
            catch
            {
                return (false, null);
            }
        }

        private string GenerateJwtToken(string username)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", username) }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _jwtIssuer,
                Audience = _jwtAudience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
} 