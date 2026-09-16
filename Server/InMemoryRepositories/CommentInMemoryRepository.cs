using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class CommentInMemoryRepository : InMemoryRepository<Comment>, ICommentRepository
{
    public CommentInMemoryRepository() : base(
        getId: c => c.ContentId,
        setId: (c, id) => c.ContentId = id)
    {
    }
}