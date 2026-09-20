namespace FileRepositories;

using Entities;
using RepositoryContracts;

public class PostFileRepository : FileRepository<Post>, IPostRepository
{
    public PostFileRepository() : base(
        filePath: "Data/posts.json",
        getId: p => p.ContentId,
        setId: (p, id) => p.ContentId = id)
    {
    }
}