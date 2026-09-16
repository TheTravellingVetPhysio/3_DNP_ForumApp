using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class ReactionInMemoryRepository : InMemoryRepository<Reaction>, IReactionRepository
{
    public ReactionInMemoryRepository() : base(
        getId: r => r.ReactionId,
        setId: (r, id) => r.ReactionId = id)
    {
    }
}