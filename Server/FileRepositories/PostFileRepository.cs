
namespace FileRepositories;

using Entities;
using ApiContracts;
using RepositoryContracts;

public class PostFileRepository : FileRepository<Post>, IPostRepository
{
    private readonly IUserRepository userRepository;
    private readonly ICommentRepository commentRepository;
    private readonly IReactionRepository reactionRepository;

    public PostFileRepository(
        IUserRepository userRepository,
        ICommentRepository commentRepository,
        IReactionRepository reactionRepository)
        : base(
            filePath: "Data/posts.json",
            getId: p => p.ContentId,
            setId: (p, id) => p.ContentId = id)
    {
        this.userRepository = userRepository;
        this.commentRepository = commentRepository;
        this.reactionRepository = reactionRepository;
    }

    // Counts

    public async Task<int> GetPostCountByUserAsync(int userId)
    {
        List<Post> posts = await GetManyAsync();

        return posts.Count(p => p.UserId == userId);
    }

    public async Task<int> GetPostCountBySubforumAsync(int subforumId)
    {
        List<Post> posts = await GetManyAsync();

        return posts.Count(p => p.SubforumId == subforumId);
    }

    // Views
    public async Task<List<PostOverviewDTO>> GetPostsOverviewAsync(
    string? title,
    int? userId,
    string? username,
    int? subforumId)
{
    List<Post> posts = await GetManyAsync();

    if (!string.IsNullOrWhiteSpace(title))
    {
        posts = posts
            .Where(p => p.Title.Contains(
                title,
                StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    if (userId.HasValue)
    {
        posts = posts
            .Where(p => p.UserId == userId.Value)
            .ToList();
    }

    if (!string.IsNullOrWhiteSpace(username))
    {
        List<User> users = await userRepository.GetManyAsync();

        HashSet<int> matchingUserIds = users
            .Where(u => u.Username.Contains(
                username,
                StringComparison.OrdinalIgnoreCase))
            .Select(u => u.UserId)
            .ToHashSet();

        posts = posts
            .Where(p => matchingUserIds.Contains(p.UserId))
            .ToList();
    }

    if (subforumId.HasValue)
    {
        posts = posts
            .Where(p => p.SubforumId == subforumId.Value)
            .ToList();
    }

    List<PostOverviewDTO> overviews =
        await CreatePostOverviewsAsync(posts);

    return overviews
        .OrderByDescending(p => p.LastActivityAt)
        .ToList();
}

    // Hjælpemetoder - dataindsamling og mapping til DTO
    private async Task<List<PostOverviewDTO>> CreatePostOverviewsAsync(
        List<Post> posts)
    {
        if (posts.Count == 0)
        {
            return new List<PostOverviewDTO>();
        }

        List<int> postIds = posts
            .Select(p => p.ContentId)
            .ToList();

        List<Comment> comments =
            await commentRepository.GetCommentsByPostIdsAsync(postIds);

        List<User> users = await userRepository.GetManyAsync();

        Dictionary<int, string> usernames = users
            .ToDictionary(u => u.UserId, u => u.Username);

        Dictionary<int, DateTimeOffset> latestCommentTimes = comments
            .GroupBy(c => c.PostId)
            .ToDictionary(
                g => g.Key,
                g => g.Max(c => c.Timestamp));

        return posts
            .Select(p => new PostOverviewDTO
            {
                PostId = p.ContentId,
                Title = p.Title,
                UserId = p.UserId,
                Username = usernames[p.UserId],
                CreatedAt = p.Timestamp,
                LastActivityAt = latestCommentTimes.TryGetValue(
                    p.ContentId, out DateTimeOffset latestCommentTime)
                    ? latestCommentTime
                    : p.Timestamp
            })
            .ToList();
    }

    public async Task<PostDetailsDTO> GetPostDetailsAsync(int postId)
    {
        Post post = await GetSingleAsync(postId);

        User postUser = await userRepository.GetSingleAsync(post.UserId);

        Dictionary<ReactionType, int> reactionCounts =
            await reactionRepository.GetReactionCountsAsync(
                post.ContentId,
                ContentType.Post);

        List<CommentDetailsDTO> commentDetails =
            await commentRepository.GetCommentDetailsAsync(postId, null, null);

        return new PostDetailsDTO
        {
            PostId = post.ContentId,
            Title = post.Title,
            Body = post.Body,
            UserId = post.UserId,
            Username = postUser.Username,
            CreatedAt = post.Timestamp,
            ReactionCounts = reactionCounts,
            Comments = commentDetails
        };
    }
}