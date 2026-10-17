namespace Services;

using Entities;
using RepositoryContracts;
using Models;

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

    public async Task UpdatePostAsync(int postId, string newTitle, string newBody)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newTitle);
        ArgumentException.ThrowIfNullOrWhiteSpace(newBody);

        Post post = await postRepository.GetSingleAsync(postId);

        TimeSpan timeSinceCreation = DateTimeOffset.UtcNow - post.Timestamp;
        if (timeSinceCreation > TimeSpan.FromHours(24))
        {
            throw new InvalidOperationException("Posten kan ikke længere redigeres, da den er mere end 24 timer gammel.");
        }

        post.Title = newTitle;
        post.Body = newBody;

        await postRepository.UpdateAsync(post);
    }

    public async Task DeletePostAsync(int postId)
    {
        await postRepository.GetSingleAsync(postId);

        int commentCount = await commentRepository.GetCommentCountByPostIdAsync(postId);

        if (commentCount == 0)
        {
            await postRepository.DeleteAsync(postId);
        }
        else
        {
            throw new InvalidOperationException("Indlægget kan ikke slettes, da det indeholder kommentarer.");
        }

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
    public Task<List<PostOverviewDTO>> ActivePostsInSubforumOverviewAsync(int subforumId)
        => postRepository.ActivePostsInSubforumOverviewAsync(subforumId);

    public Task<List<PostOverviewDTO>> GetPostsOverviewByUserAsync(int userId)
        => postRepository.GetPostsOverviewByUserAsync(userId);

    public Task<PostDetailsDTO> GetPostDetailsAsync(int postId)
        => postRepository.GetPostDetailsAsync(postId);
}