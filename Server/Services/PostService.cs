namespace Services;

using Entities;
using RepositoryContracts;
using Models;

public class PostService
{
    private readonly IPostRepository postRepository;
    private readonly UserService userService;
    private readonly SubforumService subforumService;
    private readonly ReactionService reactionService;
    private readonly CommentService commentService;

    public PostService(
        IPostRepository postRepository,
        UserService userService,
        SubforumService subforumService,
        CommentService commentService,
        ReactionService reactionService)
    {
        this.postRepository = postRepository;
        this.userService = userService;
        this.subforumService = subforumService;
        this.commentService = commentService;
        this.reactionService = reactionService;
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

        int commentCount = await commentService.GetCommentCountByPostIdAsync(postId);

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
    {
        return Task.FromResult(GetPostsByUserQuery(userId).Count());
    }

    public Task<int> GetPostCountBySubforumAsync(int subforumId)
    {
        return Task.FromResult(GetPostsBySubforumQuery(subforumId).Count());
    }


    // Views - husk at rette PostOverviewDTO alle steder ved ændringer...
    public Task<List<PostOverviewDTO>> ActivePostsInSubforumOverviewAsync(int subforumId)
        => postRepository.ActivePostsInSubforumOverviewAsync(subforumId);

    public Task<List<PostOverviewDTO>> GetPostsOverviewByUserAsync(int userId)
        => postRepository.GetPostsOverviewByUserAsync(userId);

    public async Task<PostDetailsDTO> GetPostDetailsAsync(int postId)
    {
        Post post = await postRepository.GetSingleAsync(postId);
        User postUser = await userService.GetUserByIdAsync(post.UserId);

        Dictionary<ReactionType, int> reactionCounts =
                await reactionService.GetReactionCountsAsync(post.ContentId, ContentType.Post);

        List<CommentDetailsDTO> commentDetailsDTOs = await commentService.GetCommentDetailsForPostAsync(postId);

        return new PostDetailsDTO
        {
            PostId = post.ContentId,
            Title = post.Title,
            Body = post.Body,
            UserId = post.UserId,
            Username = postUser.Username,
            CreatedAt = post.Timestamp,
            ReactionCounts = reactionCounts,
            Comments = commentDetailsDTOs
        };
    }

    // Hjælpemetoder / Queries
    private IQueryable<Post> GetAllPostsQuery()
    {
        return postRepository.GetManyAsync();
    }

    private IQueryable<Post> GetPostsByUserQuery(int userId)
    {
        return postRepository.GetManyAsync().Where(p => p.UserId == userId);
    }

    private IQueryable<Post> GetPostsBySubforumQuery(int subforumId)
    {
        return postRepository.GetManyAsync().Where(p => p.SubforumId == subforumId);
    }
}