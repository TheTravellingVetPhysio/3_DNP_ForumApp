namespace Services;

using Entities;
using RepositoryContracts;
using ApiContracts;
using Exceptions;

public class PostService
{
    private readonly IPostRepository postRepository;
    private readonly UserService userService;
    private readonly SubforumService subforumService;
    private readonly ICommentRepository commentRepository;

    public PostService(
        IPostRepository postRepository,
        UserService userService,
        SubforumService subforumService,
        ICommentRepository commentRepository)
    {
        this.postRepository = postRepository;
        this.userService = userService;
        this.subforumService = subforumService;
        this.commentRepository = commentRepository;
    }

    // CRUD
    public async Task<Post> CreatePostAsync(string title, string body, int userId, int subforumId)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Titel må ikke være tom.");
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("Indhold må ikke være tomt.");
        }

        await userService.EnsureUserExistsAsync(userId);
        await subforumService.EnsureSubforumExistsAsync(subforumId);

        Post post = new Post
        {
            Title = title,
            Body = body,
            UserId = userId,
            SubforumId = subforumId
        };

        return await postRepository.AddAsync(post);
    }

    public async Task UpdatePostAsync(int postId, string newTitle, string newBody, int currentUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newTitle);
        ArgumentException.ThrowIfNullOrWhiteSpace(newBody);

        Post post = await postRepository.GetSingleAsync(postId);

        if (post.UserId != currentUserId)
        {
            throw new ForbiddenAccessException(
                "You do now have access to alter this post.");
        }

        TimeSpan timeSinceCreation = DateTimeOffset.UtcNow - post.Timestamp;
        if (timeSinceCreation > TimeSpan.FromHours(24))
        {
            throw new BusinessConflictException("The post cannot be changed since it is older than 24 hours.");
        }

        post.Title = newTitle;
        post.Body = newBody;

        await postRepository.UpdateAsync(post);
    }

    public async Task DeletePostAsync(int postId, int currentUserId, bool isAdmin)
    {
        Post post = await postRepository.GetSingleAsync(postId);

        if (post.UserId != currentUserId && !isAdmin)
        {
            throw new ForbiddenAccessException(
                "You do not have access to delete this post.");
        }

        int commentCount =
            await commentRepository.GetCommentCountByPostIdAsync(postId);

        if (commentCount > 0 && !isAdmin)
        {
            throw new BusinessConflictException(
                "The post can't be deleted, as it contains comments.");
        }

        await postRepository.DeleteAsync(postId);
    }

    // Getters
    public async Task<Post> GetPostByIdAsync(int postId)
    {
        return await postRepository.GetSingleAsync(postId);
    }

    public Task<int> GetPostCountByUserAsync(int userId)
        => postRepository.GetPostCountByUserAsync(userId);

    public Task<int> GetPostCountBySubforumAsync(int subforumId)
        => postRepository.GetPostCountBySubforumAsync(subforumId);


    // Views - husk at rette PostOverviewDTO alle steder ved ændringer...
    public Task<List<PostOverviewDTO>> GetPostsOverviewAsync(
        string? title,
        int? userId,
        string? username,
        int? subforumId)
        => postRepository.GetPostsOverviewAsync(
            title,
            userId,
            username,
            subforumId);

    public Task<PostDetailsDTO> GetPostDetailsAsync(int postId)
        => postRepository.GetPostDetailsAsync(postId);
}