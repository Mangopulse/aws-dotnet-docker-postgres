using Microsoft.AspNetCore.Mvc;
using Shared.Interfaces;
using Shared.Services;

namespace AdminApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var (success, token, username, expiresAt) = await _authService.AuthenticateAsync(request.Username, request.Password);

                if (!success)
                {
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                return Ok(new LoginResponse
                {
                    Token = token!,
                    Username = username!,
                    ExpiresAt = expiresAt!.Value
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt for user {Username}", request.Username);
                return StatusCode(500, new { message = "Internal server error during login" });
            }
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateToken()
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var (valid, username) = await _authService.ValidateTokenAsync(token);

                if (!valid)
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                return Ok(new { valid = true, username });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return StatusCode(500, new { message = "Internal server error during token validation" });
            }
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