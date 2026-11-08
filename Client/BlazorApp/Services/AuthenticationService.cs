using System.Net;
using System.Net.Http.Headers;
using ApiContracts;

namespace BlazorApp.Services;

public class AuthenticationService(HttpClient httpClient)
    : IAuthenticationService
{
    public string? Token { get; private set; }
    public int? UserId { get; private set; }
    public string? Username { get; private set; }
    public UserRole? Role { get; private set; }
    public event Action? AuthenticationStateChanged;

    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    public async Task<bool> LoginAsync(string username, string password)
    {
        // Clear any previous session
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

        // Store session information
        Token = loginResponse.Token;
        UserId = loginResponse.UserId;
        Username = loginResponse.Username;
        Role = loginResponse.Role;

        // Automatically attach JWT to subsequent HTTP requests
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", Token);

        AuthenticationStateChanged?.Invoke();

        return true;
    }

    public void Logout()
    {
        Token = null;
        UserId = null;
        Username = null;
        Role = null;

        httpClient.DefaultRequestHeaders.Authorization = null;

        AuthenticationStateChanged?.Invoke();
    }
}