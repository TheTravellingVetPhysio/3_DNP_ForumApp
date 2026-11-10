using ApiContracts;

namespace BlazorApp.Services;

public class HttpReactionService : IReactionService
{
    private readonly HttpClient httpClient;

    public HttpReactionService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task AddReactionAsync(CreateReactionRequest request)
    {
        HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync("api/reactions", request);

        responseMessage.EnsureSuccessStatusCode();
    }

    public async Task<Dictionary<ReactionType, int>> GetReactionCountsAsync(int contentId, ContentType contentType)
    {
        string uri = $"api/reactions/counts?contentId={contentId}&contentType={contentType}";

        Dictionary<ReactionType, int>? reactions =
            await httpClient.GetFromJsonAsync<Dictionary<ReactionType, int>>(uri);

        return reactions ?? [];
    }

    public async Task RemoveReactionAsync(DeleteReactionRequest request)
    {
        HttpRequestMessage httpRequest = new HttpRequestMessage(
            HttpMethod.Delete,
            "api/reactions/")
        {
            Content = JsonContent.Create(request)
        };

        HttpResponseMessage responseMessage = await httpClient.SendAsync(httpRequest);

        responseMessage.EnsureSuccessStatusCode();
    }

    public async Task UpdateReactionAsync(UpdateReactionRequest request)
    {
        HttpResponseMessage responseMessage = await httpClient.PutAsJsonAsync("api/reactions", request);

        responseMessage.EnsureSuccessStatusCode();
    }

    public async Task<ReactionType?> GetUserReactionAsync(
        int contentId,
        ContentType contentType)
    {
        string uri = $"api/reactions/user" +
                     $"?contentId={contentId}" +
                     $"&contentType={contentType}";

        return await httpClient.GetFromJsonAsync<ReactionType?>(uri);
    }
}
