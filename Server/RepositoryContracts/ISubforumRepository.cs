using Entities;

namespace RepositoryContracts;

public interface ISubforumRepository : IRepository<Subforum>
{
    Task<bool> SubforumNameExistsAsync(string name);
}