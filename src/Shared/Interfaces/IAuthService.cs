using System.Security.Claims;

namespace Shared.Interfaces;

public interface IAuthService
{
    LoginResponse Authenticate(LoginRequest request);
    bool ValidateToken(string token);
    string GenerateJwtToken(string username);
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