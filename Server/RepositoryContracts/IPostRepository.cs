using Entities;
using ApiContracts;

namespace RepositoryContracts;

public interface IPostRepository : IRepository<Post>
{
    Task<List<PostOverviewDTO>> GetPostsOverviewAsync(
        string? title,
        int? userId,
        string? username,
        int? subforumId);
    Task<PostDetailsDTO> GetPostDetailsAsync(int postId);
    Task<int> GetPostCountByUserAsync(int userId);
    Task<int> GetPostCountBySubforumAsync(int subforumId);
}