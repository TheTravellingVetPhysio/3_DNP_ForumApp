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
}