namespace Services;

using Entities;
using RepositoryContracts;
using Services.Models;

public class PostService
{
    private readonly IPostRepository postRepository;
    private readonly IUserRepository userRepository;
    private readonly ISubforumRepository subforumRepository;
    private readonly ICommentRepository commentRepository;
    private readonly IReactionRepository reactionRepository;
    private readonly CommentService commentService;

    public PostService(
        IPostRepository postRepository,
        IUserRepository userRepository,
        ISubforumRepository subforumRepository,
        ICommentRepository commentRepository,
        IReactionRepository reactionRepository,
        CommentService commentService)
    {
        this.postRepository = postRepository;
        this.userRepository = userRepository;
        this.subforumRepository = subforumRepository;
        this.commentRepository = commentRepository;
        this.reactionRepository = reactionRepository;
        this.commentService = commentService;
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

        await userRepository.GetSingleAsync(userId);
        await subforumRepository.GetSingleAsync(subforumId);

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

    // Getters
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


    // Views - husk at rette PostOverviewDTO alle steder ved ændringer...
    public Task<List<PostSummary>> ActivePostsInSubforumOverviewAsync(int subforumId)
    {
        IQueryable<Post> posts = postRepository.GetManyAsync()
            .Where(p => p.SubforumId == subforumId);

        List<int> relevantPostIds = posts.Select(p => p.ContentId).ToList();

        IQueryable<Comment> relevantComments = commentRepository.GetManyAsync()
            .Where(c => relevantPostIds.Contains(c.PostId));

        IQueryable<User> users = userRepository.GetManyAsync();

        List<PostSummary> summaries = posts
            .Select(p => new
            {
                Post = p,
                LatestCommentTime = relevantComments
                    .Where(c => c.PostId == p.ContentId)
                    .Select(c => (DateTimeOffset?)c.Timestamp)
                    .OrderByDescending(t => t)
                    .FirstOrDefault(),
                Username = users.Single(u => u.UserId == p.UserId).Username
            })
            .OrderByDescending(x => x.LatestCommentTime ?? x.Post.Timestamp)
            .Take(20)
            .Select(x => new PostOverviewDTO
            {
                PostId = x.Post.ContentId,
                Title = x.Post.Title,
                UserId = x.Post.UserId,
                Username = x.Username,
                CreatedAt = x.Post.Timestamp,
                LastActivityAt = x.LatestCommentTime ?? x.Post.Timestamp
            })
            .ToList();

        return Task.FromResult(summaries);
    }

    public Task<List<PostSummary>> GetPostsOverviewByUserAsync(int userId)
    {
        IQueryable<Post> posts = GetPostsByUserQuery(userId);

        List<int> relevantPostIds = posts.Select(p => p.ContentId).ToList();

        IQueryable<Comment> relevantComments = commentRepository.GetManyAsync()
            .Where(c => relevantPostIds.Contains(c.PostId));

        IQueryable<User> users = userRepository.GetManyAsync();

        List<PostSummary> summaries = posts
            .Select(p => new
            {
                Post = p,
                LatestCommentTime = relevantComments
                    .Where(c => c.PostId == p.ContentId)
                    .Select(c => (DateTimeOffset?)c.Timestamp)
                    .OrderByDescending(t => t)
                    .FirstOrDefault(),
                Username = users.Single(u => u.UserId == p.UserId).Username
            })
            .Select(x => new PostOverviewDTO
            {
                PostId = x.Post.ContentId,
                Title = x.Post.Title,
                UserId = x.Post.UserId,
                Username = x.Username,
                CreatedAt = x.Post.Timestamp,
                LastActivityAt = x.LatestCommentTime ?? x.Post.Timestamp
            })
            .ToList();

        return Task.FromResult(summaries);
    }

    public async Task<PostDetailsDTO> GetPostDetailsAsync(int postId)
    {
        Post post = await postRepository.GetSingleAsync(postId);
        User postUser = await userRepository.GetSingleAsync(post.UserId);

        Dictionary<ReactionType, int> reactionCounts = reactionRepository.GetManyAsync()
            .Where(r => r.ContentId == post.ContentId && r.ContentType == ContentType.Post)
            .GroupBy(r => r.ReactionType)
            .ToDictionary(g => g.Key, g => g.Count());

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

    // User "settings"
    public Task<int> GetPostCountByUserAsync(int userId)
    {
        return Task.FromResult(GetPostsByUserQuery(userId).Count());
    }

}