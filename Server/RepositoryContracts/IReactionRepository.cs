using Entities;

namespace RepositoryContracts;

public interface IReactionRepository : IRepository<Reaction>
{
    Task<Reaction?> GetReactionByUserAndContentAsync(
        int userId,
        int contentId,
        ContentType contentType);

    Task<Dictionary<ReactionType, int>> GetReactionCountsAsync(
        int contentId,
        ContentType contentType);
}