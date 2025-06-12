using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AdminApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                // Simple hardcoded authentication for demo purposes
                // In production, this should validate against a user database with hashed passwords
                var adminUsername = _configuration["Admin:Username"] ?? "admin";
                var adminPassword = _configuration["Admin:Password"] ?? "admin123";

                if (request.Username != adminUsername || request.Password != adminPassword)
                {
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                var token = GenerateJwtToken(request.Username);
                
                return Ok(new LoginResponse
                {
                    Token = token,
                    Username = request.Username,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt for user {Username}", request.Username);
                return StatusCode(500, new { message = "Internal server error during login" });
            }
        }

        [HttpPost("validate")]
        public IActionResult ValidateToken()
        {
            // If the request reaches here, the JWT middleware has already validated the token
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            return Ok(new { valid = true, username });
        }

        private string GenerateJwtToken(string username)
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

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
} 