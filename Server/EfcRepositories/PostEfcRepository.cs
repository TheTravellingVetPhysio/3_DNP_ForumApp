namespace EfcRepositories;

using RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Entities;
using ApiContracts;
using Exceptions;

public class PostEfcRepository : EfcRepository<Post>, IPostRepository
{
    private readonly ForumAppContext ctx;
    private readonly ICommentRepository commentRepository;
    private readonly IReactionRepository reactionRepository;

    public PostEfcRepository(
        ForumAppContext ctx,
        ICommentRepository commentRepository,
        IReactionRepository reactionRepository) : base(ctx)
    {
        this.ctx = ctx;
        this.commentRepository = commentRepository;
        this.reactionRepository = reactionRepository;
    }

    // Counts
    public async Task<int> GetPostCountBySubforumAsync(int subforumId)
    {
        return await ctx.Posts.CountAsync(p => p.SubforumId == subforumId);
    }

    public async Task<int> GetPostCountByUserAsync(int userId)
    {
        return await ctx.Posts.CountAsync(p => p.UserId == userId);
    }

    // Views
    public async Task<PostDetailsDTO> GetPostDetailsAsync(int postId)
    {
        PostDetailsDTO postDetails = await ctx.Posts
            .Where(p => p.ContentId == postId)
            .Select(p => new PostDetailsDTO
            {
                PostId = p.ContentId,
                Title = p.Title,
                Body = p.Body,
                UserId = p.UserId,
                Username = p.User.Username,
                SubforumId = p.SubforumId,
                CreatedAt = p.Timestamp,
                ReactionCounts = new Dictionary<ReactionType, int>(),
                Comments = new List<CommentDetailsDTO>()
            })
            .SingleOrDefaultAsync()
            ?? throw new ResourceNotFoundException(
                $"Post with ID '{postId}' not found");

        postDetails.ReactionCounts =
            await reactionRepository.GetReactionCountsAsync(
                postId, ContentType.Post);

        postDetails.Comments =
            await commentRepository.GetCommentDetailsAsync(
                postId, null, null);

        return postDetails;
    }

    public async Task<List<PostOverviewDTO>> GetPostsOverviewAsync(
        string? title, 
        int? userId, 
        string? username, 
        int? subforumId)
    {
        IQueryable<Post> query = ctx.Posts;

        if (!string.IsNullOrWhiteSpace(title))
        {
            string searchTitle = title.ToLower();

            query = query.Where(p =>
                p.Title.ToLower().Contains(searchTitle));
        }

        if (userId.HasValue)
        {
            query = query.Where(p => p.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(username))
        {
            string searchUsername = username.ToLower();

            query = query.Where(p =>
                p.User.Username.ToLower().Contains(searchUsername));
        }

        if (subforumId.HasValue)
        {
            query = query.Where(p =>
                p.SubforumId == subforumId.Value);
        }
        
        return await query
            .Select(p => new PostOverviewDTO
            {
                PostId = p.ContentId,
                Title = p.Title,
                UserId = p.UserId,
                Username = p.User.Username,
                CreatedAt = p.Timestamp,
                LastActivityAt = p.Comments
                    .Max(c => (DateTimeOffset?)c.Timestamp) ?? p.Timestamp
            })
            .OrderByDescending(p => p.LastActivityAt)
            .ToListAsync();
    }
}