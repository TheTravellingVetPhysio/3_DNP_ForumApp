using ApiContracts;

namespace BlazorApp.Services;

public interface IAuthenticationService
{
    Task<bool> LoginAsync(string username, string password);
    void Logout();
    event Action? AuthenticationStateChanged;

    bool IsAuthenticated { get; }
    string? Token { get; }
    int? UserId { get; }
    string? Username { get; }
    UserRole? Role { get; }
}