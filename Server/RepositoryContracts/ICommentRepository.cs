using Entities;
using ApiContracts;

namespace RepositoryContracts;

public interface ICommentRepository : IRepository<Comment>
{
    Task<List<Comment>> GetCommentsByPostIdAsync(int postId);
    Task<List<Comment>> GetCommentsByPostIdsAsync(List<int> postIds);
    Task<int> GetCommentCountByPostIdAsync(int postId);
    Task<List<CommentDetailsDTO>> GetCommentDetailsAsync(
        int? postId,
        int? userId,
        string? username);
}