namespace Services;

using Entities;
using RepositoryContracts;
using Services.Models;

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

        public async Task<Post> DeletePostAsync(int postId)
    {
        Post post = GetPostByIdAsync(postId);

        if (userService.GetPostCountByPostAsync(postId) == 0)
        {
            await postRepository.DeleteAsync(post);
        }

    }

    // Getters

        public async Task<Post> GetPostByIdAsync(int postId)
    {
        return await postRepository.GetSingleAsync(postId);
    }

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

        public Task<int> GetPostCountByUserAsync(int userId)
    {
        return Task.FromResult(GetPostsByUserQuery(userId).Count());
    }

        public Task<int> GetPostCountBySubforumAsync(int subforumId)
    {
        return Task.FromResult(GetPostsBySubforumQuery(subforumId).Count());
    }


    // Views - husk at rette PostOverviewDTO alle steder ved ændringer...
    public Task<List<PostSummary>> ActivePostsInSubforumOverviewAsync(int subforumId)
    {
        IQueryable<Post> posts = postRepository.GetManyAsync()
            .Where(p => p.SubforumId == subforumId);

        List<int> relevantPostIds = posts.Select(p => p.ContentId).ToList();

        IQueryable<Comment> relevantComments = commentService.GetManyAsync()
            .Where(c => relevantPostIds.Contains(c.PostId));

        IQueryable<User> users = userService.GetManyAsync();

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

}