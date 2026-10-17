using Entities;
using Models;

namespace RepositoryContracts;

public interface IPostRepository : IRepository<Post>
{
    Task<List<PostOverviewDTO>> GetPostsOverviewByUserAsync(int userId);
    Task<List<PostOverviewDTO>> ActivePostsInSubforumOverviewAsync(
int subforumId);
    Task<PostDetailsDTO> GetPostDetailsAsync(int postId);
    Task<int> GetPostCountByUserAsync(int userId);
    Task<int> GetPostCountBySubforumAsync(int subforumId);
}