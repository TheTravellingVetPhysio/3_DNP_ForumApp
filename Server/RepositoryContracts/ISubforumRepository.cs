using ApiContracts;
using Entities;

namespace RepositoryContracts;

public interface ISubforumRepository : IRepository<Subforum>
{
    Task<bool> SubforumNameExistsAsync(string name);

    Task<List<SubforumDTO>> GetSubforumOverviewsAsync();
}