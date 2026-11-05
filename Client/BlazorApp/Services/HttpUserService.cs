using System;
using ApiContracts;

namespace BlazorApp.Services;

public class HttpUserService : IUserService
{
    private readonly HttpClient httpClient;

    public HttpUserService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<UserDTO> CreateUserAsync(CreateUserRequest request)
    {
        HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync("api/users", request);

        responseMessage.EnsureSuccessStatusCode();

        UserDTO? user = await responseMessage.Content.ReadFromJsonAsync<UserDTO>();
        
        return user ?? throw new InvalidOperationException("Server returned an empty response.");
    }

    public async Task<List<UserDTO>> GetAllUsersAsync(string? username)
    {
        string uri = "api/users";

        if (!string.IsNullOrEmpty(username))
            uri += $"?username={Uri.EscapeDataString(username)}";

        List<UserDTO>? users = await httpClient.GetFromJsonAsync<List<UserDTO>>(uri);

        return users ?? [];
    }

    public async Task<UserDTO> GetUserByIdAsync(int userId)
    {
        UserDTO? user = await httpClient.GetFromJsonAsync<UserDTO>($"api/users/{userId}");

        return user ?? throw new InvalidOperationException("Server returned an empty response.");

    }
}
