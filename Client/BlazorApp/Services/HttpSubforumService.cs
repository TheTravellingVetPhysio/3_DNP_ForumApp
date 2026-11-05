using ApiContracts;

namespace BlazorApp.Services;

public class HttpSubforumService : ISubforumService
{
    private readonly HttpClient httpClient;

    public HttpSubforumService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<SubforumDTO> CreateSubforumAsync(CreateSubforumRequest request)
    {
        HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync("api/subforums", request);
        
        responseMessage.EnsureSuccessStatusCode();

        SubforumDTO? subforum = await responseMessage.Content.ReadFromJsonAsync<SubforumDTO>();

        return subforum ?? throw new InvalidOperationException("Server returned an empty response.");

    }

    public async Task DeleteSubforumAsync(int subforumId)
    {
        HttpResponseMessage responseMessage = await httpClient.DeleteAsync($"api/subforums/{subforumId}");

        responseMessage.EnsureSuccessStatusCode();
    }

    public async Task<List<SubforumDTO>> GetAllSubforumsAsync()
    {
        List<SubforumDTO>? subforums = await httpClient.GetFromJsonAsync<List<SubforumDTO>>("api/subforums");

        return subforums ?? [];
    }

    public async Task<SubforumDTO> GetSubforumByIdAsync(int subforumId)
    {
        SubforumDTO? subforum = await httpClient.GetFromJsonAsync<SubforumDTO>($"api/subforums/{subforumId}");

        return subforum ?? throw new InvalidOperationException("Server returned an empty response.");

    }
}
