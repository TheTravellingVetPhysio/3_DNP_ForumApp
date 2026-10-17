namespace FileRepositories;

using Entities;
using Models;
using RepositoryContracts;

public class PostFileRepository : FileRepository<Post>, IPostRepository
{
    public PostFileRepository() : base(
        filePath: "Data/posts.json",
        getId: p => p.ContentId,
        setId: (p, id) => p.ContentId = id)
    {
    }

    public Task<List<PostOverviewDTO>> GetPostsOverviewByUserAsync(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<List<PostOverviewDTO>> ActivePostsInSubforumOverviewAsync(
int subforumId)
    {
        throw new NotImplementedException();
    }

    public Task<PostDetailsDTO> GetPostDetailsAsync(int postId)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetPostCountBySubforumAsync(int subforumId)
    {
        throw new NotImplementedException();
    }
}