using System.Net.Http.Json;
using ApiContracts;

namespace BlazorApp.Services;

public class HttpCommentService : ICommentService
{
    private readonly HttpClient httpClient;

    public HttpCommentService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task CreateCommentAsync(CreateCommentRequest request)
    {
        HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync("api/comments", request);
        
        responseMessage.EnsureSuccessStatusCode();
    }

    public async Task<List<CommentDetailsDTO>> GetCommentsAsync(int? postId, int? userId, string? username)
    {
        List<string> queryParams = new();

        if (postId.HasValue)
            queryParams.Add($"postId={postId}");

        if (userId.HasValue)
            queryParams.Add($"userId={userId}");

        if (!string.IsNullOrEmpty(username))
            queryParams.Add($"username={Uri.EscapeDataString(username)}");
                
        string uri = "api/comments";

        if (queryParams.Count > 0)
            uri += "?" + string.Join("&", queryParams);
        
        List<CommentDetailsDTO>? comments = await httpClient.GetFromJsonAsync<List<CommentDetailsDTO>>(uri);

        return comments ?? [];
    }

    public async Task UpdateCommentAsync(int commentId, UpdateCommentRequest request)
    {
        HttpResponseMessage responseMessage = await httpClient.PutAsJsonAsync($"api/comments/{commentId}", request);

        responseMessage.EnsureSuccessStatusCode();
    }
}
