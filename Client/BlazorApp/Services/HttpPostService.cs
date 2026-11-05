using ApiContracts;

namespace BlazorApp.Services;

public class HttpPostService : IPostService
{
    private readonly HttpClient httpClient;

    public HttpPostService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<PostDetailsDTO> CreatePostAsync(CreatePostRequest request)
    {
        HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync("api/posts", request);

        responseMessage.EnsureSuccessStatusCode();

        PostDetailsDTO? postDetails = await responseMessage.Content.ReadFromJsonAsync<PostDetailsDTO>();

        return postDetails ?? throw new InvalidOperationException("Server returned an empty response.");
    }

    public async Task DeletePostAsync(int postId)
    {
        HttpResponseMessage responseMessage = await httpClient.DeleteAsync($"api/posts/{postId}");

        responseMessage.EnsureSuccessStatusCode();
    }

    public async Task<PostDetailsDTO> GetPostDetailsByIdAsync(int postId)
    {
        PostDetailsDTO? postDetails = await httpClient.GetFromJsonAsync<PostDetailsDTO>($"api/posts/{postId}");

        return postDetails ?? throw new InvalidOperationException("Server returned an empty response.");
    }

    public async Task<List<PostOverviewDTO>> GetPostsAsync(string? title, int? userId, string? username, int? subforumId)
    {
        List<string> queryParams = new();

        if (!string.IsNullOrEmpty(title))
            queryParams.Add($"title={Uri.EscapeDataString(title)}");

        if (userId.HasValue)
            queryParams.Add($"userId={userId}");

        if (!string.IsNullOrEmpty(username))
            queryParams.Add($"username={Uri.EscapeDataString(username)}");

        if (subforumId.HasValue)
            queryParams.Add($"subforumId={subforumId}");
                
        string uri = "api/posts";

        if (queryParams.Count > 0)
            uri += "?" + string.Join("&", queryParams);

        List<PostOverviewDTO>? posts = await httpClient.GetFromJsonAsync<List<PostOverviewDTO>>(uri);

        return posts ?? [];    
        
    }

    public async Task UpdatePostAsync(int postId, UpdatePostRequest request)
    {
        HttpResponseMessage responseMessage = await httpClient.PutAsJsonAsync($"api/posts/{postId}", request);

        responseMessage.EnsureSuccessStatusCode();
    }
}
