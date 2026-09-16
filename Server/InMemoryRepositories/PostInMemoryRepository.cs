using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class PostInMemoryRepository : InMemoryRepository<Post>, IPostRepository
{
    public PostInMemoryRepository() : base(
        getId: p => p.ContentId,
        setId: (p, id) => p.ContentId = id)
    {
    }
}