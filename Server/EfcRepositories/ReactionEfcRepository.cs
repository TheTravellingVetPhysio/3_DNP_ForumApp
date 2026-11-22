namespace EfcRepositories;

using RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Entities;
using ApiContracts;

public class ReactionEfcRepository : EfcRepository<Reaction>, IReactionRepository
{
    private readonly ForumAppContext ctx;

    public ReactionEfcRepository(ForumAppContext ctx) : base(ctx)
    {
        this.ctx = ctx;
    }

    public async Task<Reaction?> GetReactionByUserAndContentAsync(
        int userId, 
        int contentId, 
        ContentType contentType)
    {
        return await ctx.Reactions
            .SingleOrDefaultAsync(r =>
                r.UserId == userId &&
                r.ContentId == contentId &&
                r.ContentType == contentType);
    }

    public async Task<Dictionary<ReactionType, int>> GetReactionCountsAsync(
        int contentId,
        ContentType contentType)
    {
        var grouped = await ctx.Reactions
            .Where(r =>
                r.ContentId == contentId &&
                r.ContentType == contentType)
            .GroupBy(r => r.ReactionType)
            .Select(g => new
            {
                ReactionType = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        return grouped.ToDictionary(
            g => g.ReactionType,
            g => g.Count);
    }

    public async Task<Dictionary<int, Dictionary<ReactionType, int>>> GetReactionCountsByContentIdsAsync(
        List<int> contentIds, 
        ContentType contentType)
    {
        var grouped = await ctx.Reactions
            .Where(r =>
                contentIds.Contains(r.ContentId) &&
                r.ContentType == contentType)
            .GroupBy(r => new {r.ContentId, r.ReactionType})
            .Select(g => new
            {
                g.Key.ContentId,
                g.Key.ReactionType,
                Count = g.Count()
            })
            .ToListAsync();

        return grouped
            .GroupBy(g => g.ContentId)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(
                    x => x.ReactionType,
                    x => x.Count));
    }
}