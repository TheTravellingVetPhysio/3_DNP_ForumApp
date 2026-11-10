using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using ApiContracts;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorApp.Services;

public class AuthenticationService(HttpClient httpClient)
    : AuthenticationStateProvider, IAuthenticationService
{
    public string? Token { get; private set; }
    public int? UserId { get; private set; }
    public string? Username { get; private set; }
    public UserRole? Role { get; private set; }

    public event Action? SessionChanged;

    private ClaimsPrincipal currentUser = new(new ClaimsIdentity());

    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(currentUser));
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        Logout();

        LoginRequest request = new()
        {
            Username = username,
            Password = password
        };

        using HttpResponseMessage response =
            await httpClient.PostAsJsonAsync("api/auth/login", request);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();

        LoginResponse? loginResponse =
            await response.Content.ReadFromJsonAsync<LoginResponse>();

        if (loginResponse == null)
        {
            throw new InvalidOperationException(
                "Server returned an empty login response.");
        }

        Token = loginResponse.Token;
        UserId = loginResponse.UserId;
        Username = loginResponse.Username;
        Role = loginResponse.Role;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, UserId.Value.ToString()),
            new(ClaimTypes.Name, Username!),
            new(ClaimTypes.Role, Role.ToString()!)
        };

        currentUser = new ClaimsPrincipal(
            new ClaimsIdentity(claims, "JWT"));

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", Token);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        SessionChanged?.Invoke();

        return true;
    }

    public void Logout()
    {
        // Clear session information
        Token = null;
        UserId = null;
        Username = null;
        Role = null;

        // Remove JWT from subsequent HTTP requests
        httpClient.DefaultRequestHeaders.Authorization = null;

        // Set current user to anonymous
        currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        // Notify Blazor and other subscribers
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        SessionChanged?.Invoke();
    }

}