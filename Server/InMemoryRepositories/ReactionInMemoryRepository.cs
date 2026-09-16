using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class ReactionInMemoryRepository : IReactionRepository
{
    private readonly List<Reaction> reactions = new();

    public Task<Reaction> AddAsync(Reaction reaction)
    {
        reaction.ReactionId = reactions.Any()
            ? reactions.Max(r => r.ReactionId) + 1
            : 1;
        reactions.Add(reaction);
        return Task.FromResult(reaction);
    }

    public Task UpdateAsync(Reaction reaction)
    {
        Reaction? existingReaction = reactions.SingleOrDefault(r => r.ReactionId == reaction.ReactionId);
        if (existingReaction is null)
        {
            throw new InvalidOperationException(
                $"Reaction with ID '{reaction.ReactionId}' not found");
        }

        reactions.Remove(existingReaction);
        reactions.Add(reaction);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        Reaction? reactionToRemove = reactions.SingleOrDefault(r => r.ReactionId == id);
        if (reactionToRemove is null)
        {
            throw new InvalidOperationException(
                $"Reaction with ID '{id}' not found");
        }

        reactions.Remove(reactionToRemove);
        return Task.CompletedTask;
    }

    public Task<Reaction> GetSingleAsync(int id)
    {
        Reaction? reaction = reactions.SingleOrDefault(r => r.ReactionId == id);
        if (reaction is null)
        {
            throw new InvalidOperationException(
                $"Reaction with ID '{id}' not found");
        }

        return Task.FromResult(reaction);
    }

    public IQueryable<Reaction> GetManyAsync()
    {
        return reactions.AsQueryable();
    }
}