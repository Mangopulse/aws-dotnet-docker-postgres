using Shared.Models;

namespace Shared.Interfaces
{
    public interface IAuthService
    {
        Task<(bool success, string? token, string? username, DateTime? expiresAt)> AuthenticateAsync(string username, string password);
        Task<(bool valid, string? username)> ValidateTokenAsync(string token);
    }
} 