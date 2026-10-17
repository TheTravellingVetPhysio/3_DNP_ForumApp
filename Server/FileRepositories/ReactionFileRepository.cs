namespace FileRepositories;

using Entities;
using RepositoryContracts;

public class ReactionFileRepository : FileRepository<Reaction>, IReactionRepository
{
    public ReactionFileRepository() : base(
        filePath: "Data/reactions.json",
        getId: r => r.ReactionId,
        setId: (r, id) => r.ReactionId = id)
    {
    }

    public async Task<Reaction?> GetReactionByUserAndContentAsync(
    int userId,
    int contentId,
    ContentType contentType)
    {
        List<Reaction> reactions = await GetManyAsync();

        return reactions.SingleOrDefault(r =>
            r.UserId == userId &&
            r.ContentId == contentId &&
            r.ContentType == contentType);
    }

    public async Task<Dictionary<ReactionType, int>> GetReactionCountsAsync(
        int contentId,
        ContentType contentType)
    {
        List<Reaction> reactions = await GetManyAsync();

        return reactions
            .Where(r =>
                r.ContentId == contentId &&
                r.ContentType == contentType)
            .GroupBy(r => r.ReactionType)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}