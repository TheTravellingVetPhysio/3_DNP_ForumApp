
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

    public async Task<List<PostOverviewDTO>> GetPostsOverviewByUserAsync(
        int userId)
    {
        List<Post> posts = (await GetManyAsync())
            .Where(p => p.UserId == userId)
            .ToList();

        return await CreatePostOverviewsAsync(posts);
    }

    public async Task<List<PostOverviewDTO>> ActivePostsInSubforumOverviewAsync(
        int subforumId)
    {
        List<Post> posts = (await GetManyAsync())
            .Where(p => p.SubforumId == subforumId)
            .ToList();

        List<PostOverviewDTO> summaries =
            await CreatePostOverviewsAsync(posts);

        return summaries
            .OrderByDescending(p => p.LastActivityAt)
            .Take(20)
            .ToList();
    }

    // Shared logic for creating post overviews

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
            await commentRepository.GetCommentDetailsForPostAsync(postId);

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