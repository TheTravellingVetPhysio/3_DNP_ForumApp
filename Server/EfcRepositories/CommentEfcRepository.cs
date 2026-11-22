namespace EfcRepositories;

using RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Entities;
using ApiContracts;

public class CommentEfcRepository : EfcRepository<Comment>, ICommentRepository
{
    private readonly ForumAppContext ctx;
    private readonly IReactionRepository reactionRepository;

    public CommentEfcRepository(
        ForumAppContext ctx,
        IReactionRepository reactionRepository) : base(ctx)
    {
        this.ctx = ctx;
        this.reactionRepository = reactionRepository;
    }

    public async Task<int> GetCommentCountByPostIdAsync(int postId)
    {
        return await ctx.Comments.CountAsync(c => c.PostId == postId);
    }

    public async Task<List<CommentDetailsDTO>> GetCommentDetailsAsync(
        int? postId, 
        int? userId, 
        string? username)
    {
        
        IQueryable<Comment> query = ctx.Comments;
        if (postId.HasValue)
        {
            query = query.Where(c => c.PostId == postId.Value);
        }
        
        if (userId.HasValue)
        {
            query = query.Where(c => c.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(username))
        {
            string searchUsername = username.ToLower();

            query = query.Where(c =>
                c.User.Username.ToLower().Contains(searchUsername));
        }

        List<CommentDetailsDTO> result = await query
            .Select(c => new CommentDetailsDTO
            {
                CommentId = c.ContentId,
                PostId = c.PostId,
                Body = c.Body,
                UserId = c.UserId,
                Username = c.User.Username,
                CreatedAt = c.Timestamp,
                ParentCommentId = c.ParentCommentId,
                ReactionCounts = new Dictionary<ReactionType, int>()
            })
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        List<int> commentIds = result
            .Select(c => c.CommentId)
            .ToList();

        var reactionCounts = await reactionRepository
            .GetReactionCountsByContentIdsAsync(
                commentIds, ContentType.Comment);

        foreach (CommentDetailsDTO comment in result)
        {
            if (reactionCounts.TryGetValue(
                comment.CommentId, out var counts))
            {
                comment.ReactionCounts = counts;
            }
        }

        return result;
    }

    public async Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
    {
        return await ctx.Comments
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.Timestamp)
            .ToListAsync();
    }

    public async Task<List<Comment>> GetCommentsByPostIdsAsync(List<int> postIds)
    {
        return await ctx.Comments
            .Where(c => postIds.Contains(c.PostId))
            .OrderBy(c => c.Timestamp)
            .ToListAsync();
    }
}