using Entities;

namespace RepositoryContracts;

public interface IReactionRepository
{
    Task<Reaction> AddAsync(Reaction reaction);
    Task UpdateAsync(Reaction reaction);
    Task DeleteAsync(int id);
    Task<Reaction> GetSingleAsync(int id);
    IQueryable<Reaction> GetManyAsync();
}