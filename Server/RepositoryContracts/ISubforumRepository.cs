using Entities;

namespace RepositoryContracts;

public interface ISubforumRepository
{
    Task<Subforum> AddAsync(Subforum subforum);
    Task UpdateAsync(Subforum subforum);
    Task DeleteAsync(int id);
    Task<Subforum> GetSingleAsync(int id);
    IQueryable<Subforum> GetManyAsync();
}