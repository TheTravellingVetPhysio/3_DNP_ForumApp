namespace FileRepositories;

using Entities;
using RepositoryContracts;
using Models;

public class CommentFileRepository : FileRepository<Comment>, ICommentRepository
{
    public CommentFileRepository() : base(
        filePath: "Data/comments.json",
        getId: c => c.ContentId,
        setId: (c, id) => c.ContentId = id)
    {
    }

    public Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Comment>> GetCommentsByPostIdsAsync(List<int> postIds)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetCommentCountByPostIdAsync(int postId)
    {
        throw new NotImplementedException();
    }

    public Task<List<CommentDetailsDTO>> GetCommentDetailsForPostAsync(int postId)
    {
        throw new NotImplementedException();
    }
}