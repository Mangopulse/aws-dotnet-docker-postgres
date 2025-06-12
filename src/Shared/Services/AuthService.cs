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
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<(bool success, string? token, string? username, DateTime? expiresAt)> AuthenticateAsync(string username, string password)
        {
            if (username == "admin" && password == "admin123")
            {
                var token = GenerateJwtToken(username, username);
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
            var key = Encoding.UTF8.GetBytes(_configuration["JWT:Key"] ?? throw new InvalidOperationException("JWT:Key is not configured"));

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JWT:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["JWT:Audience"],
                    ClockSkew = TimeSpan.Zero
                }, out _);

                var username = principal?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
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

        public string GenerateJwtToken(string userId, string username)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["JWT:Key"] ?? throw new InvalidOperationException("JWT:Key is not configured")));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, username)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JWT:Key"] ?? throw new InvalidOperationException("JWT:Key is not configured"));

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JWT:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["JWT:Audience"],
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return principal != null;
            }
            catch
            {
                return false;
            }
        }
    }
} 