using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class SubforumInMemoryRepository : InMemoryRepository<Subforum>, ISubforumRepository
{
    public SubforumInMemoryRepository() : base(
        getId: s => s.SubforumId,
        setId: (s, id) => s.SubforumId = id)
    {
    }
}