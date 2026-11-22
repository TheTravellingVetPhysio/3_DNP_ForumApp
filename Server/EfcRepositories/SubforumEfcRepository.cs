namespace EfcRepositories;

using RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Entities;
using ApiContracts;

public class SubforumEfcRepository : EfcRepository<Subforum>, ISubforumRepository
{
    private readonly ForumAppContext ctx;

    public SubforumEfcRepository(ForumAppContext ctx) : base(ctx)
    {
        this.ctx = ctx;
    }

    public async Task<List<SubforumDTO>> GetSubforumOverviewsAsync()
    {
        return await ctx.Subforums
            .Select(s => new SubforumDTO
            {
                SubforumId = s.SubforumId,
                Name = s.Name,
                LastActivityAt = s.Posts
                    .Select(p => (DateTimeOffset?)p.Timestamp)
                    .Concat(s.Posts
                        .SelectMany(p => p.Comments)
                            .Select(c => (DateTimeOffset?)c.Timestamp))
                    .Max()
            })
            .ToListAsync();
    }

    public async Task<bool> SubforumNameExistsAsync(string name)
    {
        return await ctx.Subforums.AnyAsync(s =>
            s.Name.ToLower() == name.ToLower());
    }
}