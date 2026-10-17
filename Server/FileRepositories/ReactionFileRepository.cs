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

    public Task<Reaction?> GetReactionByUserAndContentAsync(
        int userId,
        int contentId,
        ContentType contentType)
    {
        throw new NotImplementedException();
    }

    public Task<Dictionary<ReactionType, int>> GetReactionCountsAsync(
        int contentId,
        ContentType contentType)
    {
        throw new NotImplementedException();
    }
}