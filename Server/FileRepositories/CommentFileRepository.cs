namespace FileRepositories;

using Entities;
using RepositoryContracts;

public class CommentFileRepository : FileRepository<Comment>, ICommentRepository
{
    public CommentFileRepository() : base(
        filePath: "Data/comments.json",
        getId: c => c.ContentId,
        setId: (c, id) => c.ContentId = id)
    {
    }
}